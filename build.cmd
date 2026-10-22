@echo off
rem Build obs-vbcable-audio-output.dll with the MSVC toolchain.
rem Run from an "x64 Native Tools Command Prompt for VS 2022".
rem Requires the OBS headers in third_party\obs-studio (see docs\BUILD.md).
setlocal
set "ROOT=%~dp0"
set "OBS_HEADERS=%ROOT%third_party\obs-studio\libobs"
set "OUT=%ROOT%build"

if not exist "%OBS_HEADERS%\obs-module.h" (
  echo OBS headers missing: %OBS_HEADERS%
  echo Run: git clone --depth 1 --branch 32.2.2 https://github.com/obsproject/obs-studio.git third_party\obs-studio
  exit /b 1
)
if not exist "%OUT%" mkdir "%OUT%"

lib /nologo /def:"%ROOT%src\obs-imports.def" /machine:x64 /out:"%OUT%\obs-imports.lib" || exit /b 1
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /Fo"%OUT%\\" /I"%ROOT%include" /I"%OBS_HEADERS%" "%ROOT%src\obs-vbcable-audio-output.cpp" ^
  /link /OUT:"%OUT%\obs-vbcable-audio-output.dll" /IMPLIB:"%OUT%\obs-vbcable-audio-output.lib" /LIBPATH:"%OUT%" obs-imports.lib ole32.lib uuid.lib || exit /b 1

echo.
echo Built: %OUT%\obs-vbcable-audio-output.dll
