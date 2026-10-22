# OBS VB-CABLE Audio Output

OBS-Studio-Plugin für Windows, das den **fertigen OBS-Hauptmix direkt auf das virtuelle Audiokabel [VB-Audio Virtual Cable](https://vb-audio.com/Cable/) ausgibt** – ohne OBS-Audio-Monitoring.

Dadurch kann jede Anwendung (Browser, Discord, Zoom, Web-Streaming-Seiten …) den OBS-Ton einfach als Mikrofon `CABLE Output` verwenden.

```text
OBS-Hauptmix ──► Plugin ──► CABLE Input (VB-Audio Virtual Cable) ══► CABLE Output ──► beliebige Anwendung
```

## Warum?

Der übliche Weg („Audio-Monitoring auf CABLE Input stellen“) hat Nachteile: Das Monitoring-Gerät ist global, man verliert das Mithören über Kopfhörer, und jede Quelle muss einzeln auf „Monitor und Ausgabe“ gestellt werden. Dieses Plugin greift stattdessen den Mix über die offizielle OBS-Schnittstelle `obs_add_raw_audio_callback` ab und schreibt ihn per WASAPI auf *CABLE Input*. OBS-Einstellungen werden nicht verändert.

## Funktionen

- Ausgabe des OBS-Hauptmixes (Spur 1), 48 kHz, Stereo, 32-Bit-Float
- Findet *CABLE Input* automatisch
- Blockiert den OBS-Audio-Thread nie (lock-freier Ringpuffer, eigener Ausgabe-Thread)
- Begrenzte Latenz (~20–170 ms); Puffer wird bei Taktabweichung automatisch gekürzt
- Keine Netzwerkverbindung, keine Oberfläche, keine Konfiguration
- Meldungen im OBS-Log mit dem Präfix `[vbcable-output]`

## Installation

👉 **Ausführliche Schritt-für-Schritt-Anleitung inkl. VB-CABLE: [INSTALLATION.md](INSTALLATION.md)**

Kurzfassung:

1. VB-CABLE von <https://vb-audio.com/Cable/> laden, entpacken, `VBCABLE_Setup_x64.exe` als Administrator ausführen → *Install Driver* → PC neu starten.
2. `obs-vbcable-audio-output.zip` unter [Releases](../../releases) laden und entpacken.
3. OBS schließen, `Install-AlsAdmin.cmd` doppelklicken.
4. OBS starten; in der Ziel-Anwendung `CABLE Output (VB-Audio Virtual Cable)` als Mikrofon wählen.

## Kompatibilität

| | |
|---|---|
| Betriebssystem | Windows 10 / 11, 64 Bit |
| OBS Studio | 32.x (gebaut gegen 32.2.2) |
| Virtuelles Kabel | VB-Audio Virtual Cable (`CABLE Input …`) |

## Selbst bauen

Siehe [docs/BUILD.md](docs/BUILD.md). Jeder Push wird per GitHub Actions auf Windows gebaut; Tags `v*` erzeugen automatisch ein Release mit ZIP.

## Projektstruktur

```text
src/        Plugin-Quellcode (C++17, WASAPI)
include/    minimale obsconfig.h für den Build ohne OBS-Source-Tree-Build
installer/  Install-/Uninstall-Skripte (PowerShell + CMD)
docs/       Build-Anleitung
.github/    CI-Workflow (Build + Release)
```

## Lizenz

GPL-2.0 (wie OBS Studio, gegen dessen `libobs` das Plugin linkt). Siehe [LICENSE](LICENSE).

VB-CABLE ist ein Produkt von VB-Audio Software und nicht Teil dieses Projekts.
