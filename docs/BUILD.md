# Selbst bauen

Das Plugin braucht keinen kompletten OBS-Build. Es reichen die **libobs-Header** und eine Import-Bibliothek, die aus `src/obs-imports.def` erzeugt wird (sie enthält nur die drei benötigten Funktionen aus `obs.dll`).

## Voraussetzungen

- Visual Studio 2022 (oder Build Tools) mit **Desktopentwicklung mit C++**
- Git

## Schritte

1. **x64 Native Tools Command Prompt for VS 2022** öffnen.
2. Repository klonen und die OBS-Header holen (Version passend zur installierten OBS-Version):

   ```bat
   git clone https://github.com/<user>/obs-vbcable-audio-output.git
   cd obs-vbcable-audio-output
   git clone --depth 1 --branch 32.2.2 https://github.com/obsproject/obs-studio.git third_party\obs-studio
   ```

3. Bauen:

   ```bat
   build.cmd
   ```

   Ergebnis: `build\obs-vbcable-audio-output.dll`

4. Testen: DLL nach `C:\Program Files\obs-studio\obs-plugins\64bit\` kopieren (oder `installer\Install.ps1` neben die DLL legen und ausführen).

## Release erstellen

```bash
git tag v0.2.0
git push origin v0.2.0
```

GitHub Actions baut die DLL auf `windows-latest`, packt sie mit Installer und Anleitung in `obs-vbcable-audio-output.zip` und hängt sie an das Release.

## Funktionsweise

- `obs_module_load` registriert per `obs_add_raw_audio_callback(0, …)` einen Callback auf Mix 0 (Spur 1), angefordert als 48 kHz, Stereo, Float planar.
- Der Callback kopiert jeden Block (max. 1024 Frames) nur in einen lock-freien Ringpuffer (128 Blöcke) und kehrt sofort zurück. Ist der Puffer voll, wird verworfen statt zu blockieren.
- Ein eigener Thread öffnet *CABLE Input* per WASAPI (Shared Mode, Event-gesteuert, `AUTOCONVERTPCM`) und schreibt die Blöcke interleaved hinaus. Fehlen Daten, wird Stille geschrieben.
- Liegen mehr als 8 Blöcke im Puffer (Taktdrift zwischen OBS und Kabel), springt der Leser nach vorn, um die Latenz zu begrenzen.
