// OBS VB-CABLE Audio Output
//
// Taps the finished OBS master mix (track 1) through the official
// obs_add_raw_audio_callback API and plays it directly into the
// "CABLE Input (VB-Audio Virtual Cable)" playback endpoint via WASAPI.
// Any application can then use "CABLE Output" as its microphone.
//
// OBS audio monitoring is neither used nor changed.

#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <mmdeviceapi.h>
#include <audioclient.h>
#include <functiondiscoverykeys_devpkey.h>
#include <ksmedia.h>
#include <propvarutil.h>
#include <wrl/client.h>
#include <obs-module.h>
#include <algorithm>
#include <array>
#include <atomic>
#include <cwchar>
#include <thread>

using Microsoft::WRL::ComPtr;
OBS_DECLARE_MODULE()

#define LOG_PREFIX "[vbcable-output] "

namespace {
// Matched as a prefix so that "CABLE Input (VB-Audio Virtual Cable)" and
// slightly different driver spellings are found.
constexpr wchar_t kEndpointPrefix[] = L"CABLE Input";
constexpr uint32_t kRate = 48000, kBlockFrames = 1024;
constexpr uint16_t kChannels = 2;
constexpr size_t kBlocks = 128;
// If more blocks than this are queued (clock drift between OBS and the
// virtual cable), old blocks are dropped to keep latency low (~170 ms).
constexpr size_t kMaxQueuedBlocks = 8;

struct Block {
  uint32_t frames = 0;
  std::array<float, kBlockFrames * kChannels> data{};
};
std::array<Block, kBlocks> queue;
std::atomic<size_t> write_index{0}, read_index{0};
std::atomic<bool> running{false};
std::thread output_thread;

size_t queued() {
  const auto w = write_index.load(std::memory_order_acquire);
  const auto r = read_index.load(std::memory_order_relaxed);
  return (w + kBlocks - r) % kBlocks;
}

bool pop(Block &out) {
  auto read = read_index.load(std::memory_order_relaxed);
  if (read == write_index.load(std::memory_order_acquire)) return false;
  if (queued() > kMaxQueuedBlocks) {
    read = (write_index.load(std::memory_order_acquire) + kBlocks - kMaxQueuedBlocks / 2) % kBlocks;
  }
  out = queue[read];
  read_index.store((read + 1) % kBlocks, std::memory_order_release);
  return true;
}

ComPtr<IMMDevice> find_endpoint() {
  ComPtr<IMMDeviceEnumerator> enumerator;
  if (FAILED(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_ALL, IID_PPV_ARGS(&enumerator)))) return nullptr;
  ComPtr<IMMDeviceCollection> devices;
  if (FAILED(enumerator->EnumAudioEndpoints(eRender, DEVICE_STATE_ACTIVE, &devices))) return nullptr;
  UINT count = 0;
  devices->GetCount(&count);
  const size_t prefix_len = wcslen(kEndpointPrefix);
  for (UINT i = 0; i < count; ++i) {
    ComPtr<IMMDevice> device;
    ComPtr<IPropertyStore> store;
    PROPVARIANT value;
    PropVariantInit(&value);
    const bool match = SUCCEEDED(devices->Item(i, &device)) && SUCCEEDED(device->OpenPropertyStore(STGM_READ, &store)) &&
                       SUCCEEDED(store->GetValue(PKEY_Device_FriendlyName, &value)) && value.vt == VT_LPWSTR &&
                       wcsncmp(value.pwszVal, kEndpointPrefix, prefix_len) == 0;
    if (match) blog(LOG_INFO, LOG_PREFIX "Using playback endpoint: %ls", value.pwszVal);
    PropVariantClear(&value);
    if (match) return device;
  }
  return nullptr;
}

WAVEFORMATEXTENSIBLE format() {
  WAVEFORMATEXTENSIBLE f{};
  f.Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
  f.Format.nChannels = kChannels;
  f.Format.nSamplesPerSec = kRate;
  f.Format.wBitsPerSample = 32;
  f.Format.nBlockAlign = kChannels * sizeof(float);
  f.Format.nAvgBytesPerSec = kRate * f.Format.nBlockAlign;
  f.Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);
  f.Samples.wValidBitsPerSample = 32;
  f.dwChannelMask = SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT;
  f.SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
  return f;
}

void output_audio() {
  if (FAILED(CoInitializeEx(nullptr, COINIT_MULTITHREADED))) {
    blog(LOG_ERROR, LOG_PREFIX "COM initialization failed.");
    return;
  }
  {
    const auto device = find_endpoint();
    if (!device) {
      blog(LOG_ERROR, LOG_PREFIX "VB-CABLE playback endpoint 'CABLE Input' not found. Is VB-Audio Virtual Cable installed and enabled?");
      CoUninitialize();
      return;
    }
    ComPtr<IAudioClient> client;
    ComPtr<IAudioRenderClient> renderer;
    HANDLE event = CreateEventW(nullptr, FALSE, FALSE, nullptr);
    const auto f = format();
    HRESULT hr = device->Activate(__uuidof(IAudioClient), CLSCTX_ALL, nullptr, reinterpret_cast<void **>(client.GetAddressOf()));
    if (SUCCEEDED(hr))
      hr = client->Initialize(AUDCLNT_SHAREMODE_SHARED,
                              AUDCLNT_STREAMFLAGS_EVENTCALLBACK | AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM | AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY,
                              0, 0, &f.Format, nullptr);
    if (SUCCEEDED(hr)) hr = client->SetEventHandle(event);
    if (SUCCEEDED(hr)) hr = client->GetService(IID_PPV_ARGS(&renderer));
    if (FAILED(hr)) {
      blog(LOG_ERROR, LOG_PREFIX "WASAPI initialization failed (HRESULT 0x%08lX).", static_cast<unsigned long>(hr));
      CloseHandle(event);
      CoUninitialize();
      return;
    }
    UINT32 buffer = 0;
    client->GetBufferSize(&buffer);
    client->Start();
    blog(LOG_INFO, LOG_PREFIX "Audio output started (48 kHz, stereo, float).");

    Block current;
    uint32_t position = 0;
    while (running.load(std::memory_order_acquire)) {
      WaitForSingleObject(event, 100);
      UINT32 padding = 0;
      if (FAILED(client->GetCurrentPadding(&padding))) break;
      const UINT32 count = buffer - padding;
      if (!count) continue;
      BYTE *raw = nullptr;
      if (FAILED(renderer->GetBuffer(count, &raw))) break;
      auto *samples = reinterpret_cast<float *>(raw);
      for (UINT32 frame = 0; frame < count; ++frame) {
        if (position >= current.frames) {
          if (!pop(current)) {
            samples[frame * 2] = samples[frame * 2 + 1] = 0.0f;
            continue;
          }
          position = 0;
        }
        samples[frame * 2] = current.data[position * 2];
        samples[frame * 2 + 1] = current.data[position * 2 + 1];
        ++position;
      }
      renderer->ReleaseBuffer(count, 0);
    }
    client->Stop();
    CloseHandle(event);
    blog(LOG_INFO, LOG_PREFIX "Audio output stopped.");
  }
  CoUninitialize();
}

void raw_audio(void *, size_t, audio_data *audio) {
  if (!audio || !audio->data[0] || !audio->data[1]) return;
  const auto write = write_index.load(std::memory_order_relaxed), next = (write + 1) % kBlocks;
  if (next == read_index.load(std::memory_order_acquire)) return; // never block the OBS audio thread
  auto &block = queue[write];
  block.frames = std::min(audio->frames, kBlockFrames);
  const auto *left = reinterpret_cast<const float *>(audio->data[0]);
  const auto *right = reinterpret_cast<const float *>(audio->data[1]);
  for (uint32_t i = 0; i < block.frames; ++i) {
    block.data[i * 2] = left[i];
    block.data[i * 2 + 1] = right[i];
  }
  write_index.store(next, std::memory_order_release);
}
} // namespace

MODULE_EXPORT const char *obs_module_name() { return "VB-CABLE Audio Output"; }
MODULE_EXPORT const char *obs_module_description() { return "Routes the OBS master mix directly to VB-CABLE without OBS monitoring."; }

bool obs_module_load() {
  audio_convert_info info{};
  info.samples_per_sec = kRate;
  info.format = AUDIO_FORMAT_FLOAT_PLANAR;
  info.speakers = SPEAKERS_STEREO;
  info.allow_clipping = true;
  obs_add_raw_audio_callback(0, &info, raw_audio, nullptr);
  running.store(true);
  output_thread = std::thread(output_audio);
  blog(LOG_INFO, LOG_PREFIX "Module loaded.");
  return true;
}

void obs_module_unload() {
  obs_remove_raw_audio_callback(0, raw_audio, nullptr);
  running.store(false);
  if (output_thread.joinable()) output_thread.join();
}
