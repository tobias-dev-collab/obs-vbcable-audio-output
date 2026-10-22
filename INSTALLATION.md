# Installationsanleitung

Diese Anleitung führt dich Schritt für Schritt durch die komplette Einrichtung:

1. VB-Audio Virtual Cable (VB-CABLE) installieren
2. VB-CABLE richtig einstellen
3. Das OBS-Plugin installieren
4. OBS und die Ziel-Anwendung einrichten
5. Testen

**Voraussetzungen**

- Windows 10 oder Windows 11 (64 Bit)
- OBS Studio 64 Bit, getestet mit Version **32.2.2** ([obsproject.com](https://obsproject.com/de/download))
- Administratorrechte auf dem PC

So fließt der Ton am Ende:

```text
OBS-Hauptmix ──► Plugin ──► CABLE Input (VB-Audio Virtual Cable) ══► CABLE Output ──► Ziel-Anwendung (als Mikrofon)
```

---

## 1. VB-Audio Virtual Cable installieren

VB-CABLE ist ein kostenloser (Donationware) virtueller Audio-Treiber von VB-Audio. Er stellt zwei neue Geräte bereit:

| Gerät | Typ in Windows | Wofür |
|---|---|---|
| `CABLE Input (VB-Audio Virtual Cable)` | Wiedergabegerät (Lautsprecher) | Hierhin schreibt das Plugin den OBS-Ton |
| `CABLE Output (VB-Audio Virtual Cable)` | Aufnahmegerät (Mikrofon) | Das wählst du in der Ziel-Anwendung als Mikrofon |

Alles, was auf *CABLE Input* abgespielt wird, kommt auf *CABLE Output* wieder heraus.

### 1.1 Herunterladen

1. Öffne die offizielle Seite **<https://vb-audio.com/Cable/>**.
2. Klicke bei **Windows** auf den Download des aktuellen Treiberpakets (Stand Oktober 2026: `VBCABLE_Driver_Pack45.zip`).
   Lade VB-CABLE nur von vb-audio.com herunter, nicht von Drittseiten.

### 1.2 Entpacken

1. Rechtsklick auf die heruntergeladene ZIP-Datei → **Alle extrahieren…** → **Extrahieren**.
2. **Wichtig:** Die Setup-Datei nicht direkt aus der ZIP starten. Ohne Entpacken findet das Setup seine Treiberdateien nicht.

### 1.3 Treiber installieren

1. Öffne den entpackten Ordner.
2. Rechtsklick auf **`VBCABLE_Setup_x64.exe`** → **Als Administrator ausführen**.
   - Auf einem 32-Bit-Windows wäre es `VBCABLE_Setup.exe`; für OBS 64 Bit brauchst du aber ein 64-Bit-Windows.
3. Die Windows-Abfrage (Benutzerkontensteuerung) mit **Ja** bestätigen.
4. Im VB-CABLE-Fenster auf **Install Driver** klicken.
5. Falls Windows fragt, ob die Gerätesoftware von *VB-Audio Software* installiert werden soll: **Installieren**.
6. Die Meldung „Installation complete“ mit **OK** bestätigen.
7. **PC neu starten.** Das ist laut VB-Audio nötig, sonst fehlen die Geräte oder funktionieren nicht richtig.

### 1.4 Prüfen, ob VB-CABLE da ist

1. `Win` + `R` drücken, **`mmsys.cpl`** eingeben, Enter. Das klassische Fenster „Sound“ öffnet sich.
2. Reiter **Wiedergabe**: Dort muss **CABLE Input – VB-Audio Virtual Cable** stehen.
3. Reiter **Aufnahme**: Dort muss **CABLE Output – VB-Audio Virtual Cable** stehen.

Fehlt eines der Geräte: Rechtsklick in die Liste → **Deaktivierte Geräte anzeigen**. Ist es deaktiviert, Rechtsklick darauf → **Aktivieren**. Fehlt es ganz, Installation (Schritt 1.3) wiederholen und neu starten.

---

## 2. VB-CABLE einstellen

### 2.1 Standardgerät NICHT ändern

Windows macht nach der Installation manchmal **CABLE Input** zum Standard-Wiedergabegerät. Dann hörst du nichts mehr aus deinen Lautsprechern.

1. Im Fenster „Sound“ (`mmsys.cpl`), Reiter **Wiedergabe**: deine echten Lautsprecher/Kopfhörer anklicken → **Als Standard**.
2. Reiter **Aufnahme**: dein echtes Mikrofon anklicken → **Als Standard**.

**CABLE Input** und **CABLE Output** sollen *kein* Standardgerät sein. Das Plugin findet *CABLE Input* selbst.

### 2.2 Abtastrate auf 48 kHz stellen (empfohlen)

OBS arbeitet normalerweise mit 48 kHz. Gleiche Abtastraten vermeiden Umrechnung und Knackser.

1. Reiter **Wiedergabe** → Doppelklick auf **CABLE Input** → Reiter **Erweitert**.
2. Standardformat: **2 Kanal, 24 Bit, 48000 Hz (Studioqualität)** (oder 16 Bit, 48000 Hz) → **Übernehmen**.
3. Reiter **Aufnahme** → Doppelklick auf **CABLE Output** → Reiter **Erweitert** → ebenfalls **48000 Hz** → **Übernehmen**.
4. In OBS prüfen: **Datei → Einstellungen → Audio → Abtastrate = 48 kHz**.

---

## 3. Das OBS-Plugin installieren

### 3.1 Herunterladen

1. Im GitHub-Repository rechts auf **Releases** klicken.
2. Beim neuesten Release die Datei **`obs-vbcable-audio-output.zip`** herunterladen.
3. Rechtsklick auf die ZIP → **Eigenschaften** → falls unten „Zulassen“ angezeigt wird, Haken setzen → **OK**.
4. Rechtsklick → **Alle extrahieren…**.

Die ZIP enthält:

| Datei | Zweck |
|---|---|
| `obs-vbcable-audio-output.dll` | Das Plugin |
| `Install-AlsAdmin.cmd` | Installation per Doppelklick |
| `Install.ps1` | Eigentlicher Installer (wird von der CMD gestartet) |
| `Uninstall-AlsAdmin.cmd` / `Uninstall.ps1` | Deinstallation |

### 3.2 Installieren (automatisch)

1. **OBS vollständig schließen** – auch das Symbol unten rechts im Infobereich.
2. Im entpackten Ordner **`Install-AlsAdmin.cmd`** doppelklicken.
3. Falls Windows SmartScreen „Der Computer wurde durch Windows geschützt“ meldet: **Weitere Informationen → Trotzdem ausführen**.
4. Die Administratorabfrage mit **Ja** bestätigen.
5. Ein PowerShell-Fenster zeigt „Plugin installiert: …“. Fenster schließen.

Der Installer

- findet den OBS-Ordner automatisch (Registry, sonst `C:\Program Files\obs-studio`),
- kopiert die DLL nach `…\obs-studio\obs-plugins\64bit\`,
- sichert eine schon vorhandene Version als `.bak`,
- deaktiviert eine alte Vorgängerversion des Plugins (`*.dll` → `*.dll.disabled`), damit der Ton nicht doppelt ausgegeben wird,
- warnt, wenn VB-CABLE nicht gefunden wird.

### 3.3 Installieren (manuell, alternativ)

1. OBS schließen.
2. `obs-vbcable-audio-output.dll` nach `C:\Program Files\obs-studio\obs-plugins\64bit\` kopieren (Administratorrechte bestätigen).

---

## 4. OBS und Ziel-Anwendung einrichten

### 4.1 OBS

1. OBS starten. Das Plugin läuft automatisch, es gibt keine eigene Oberfläche.
2. Ausgegeben wird der **OBS-Hauptmix (Spur 1)** – also genau das, was du im Audio-Mixer hörst bzw. was gestreamt wird (Mikrofon, Desktop-Audio, Medienquellen usw., inkl. Filter und Lautstärke).
3. **Kein Audio-Monitoring nötig.** Stelle bei deinen Quellen unter *Erweiterte Audioeigenschaften* nichts auf „Monitor und Ausgabe“, nur damit es im Kabel landet – das Plugin nimmt den Mix direkt ab.
4. **Echo vermeiden:** Füge in OBS **CABLE Output** nicht als Audioquelle hinzu und stelle unter *Einstellungen → Audio → Monitoring-Gerät* nicht *CABLE Input* ein. Sonst entsteht eine Rückkopplung.

### 4.2 Ziel-Anwendung (Browser, Discord, Zoom, Streaming-Seite …)

1. In der Anwendung als **Mikrofon / Audio-Eingang**: **`CABLE Output (VB-Audio Virtual Cable)`** wählen.
2. Bei Webseiten im Browser: Wenn die Seite nach dem Mikrofon fragt, oben links auf das Schloss-/Einstellungssymbol neben der Adresse klicken → Mikrofon → **CABLE Output** wählen, ggf. Seite neu laden.
   In Chrome/Edge geht das auch global unter `chrome://settings/content/microphone` bzw. `edge://settings/content/microphone`.
3. Falls die Anwendung „Rauschunterdrückung“, „Echo-Unterdrückung“ oder „Automatische Lautstärke“ anbietet: am besten **ausschalten**, OBS liefert schon fertig gemischten Ton.

---

## 5. Testen

1. Im Fenster „Sound“ (`mmsys.cpl`), Reiter **Aufnahme**: Bei **CABLE Output** muss der grüne Pegel ausschlagen, sobald in OBS Ton läuft (z. B. ins Mikrofon sprechen).
2. Selbst mithören (optional): Doppelklick auf **CABLE Output** → Reiter **Abhören** → **Dieses Gerät als Wiedergabequelle verwenden** → deine Kopfhörer wählen → **Übernehmen**. Nach dem Test den Haken wieder entfernen.
3. OBS-Log prüfen: **Hilfe → Log-Dateien → Aktuelle Log-Datei anzeigen** und nach **`[vbcable-output]`** suchen. Erwartet:

   ```text
   [vbcable-output] Module loaded.
   [vbcable-output] Using playback endpoint: CABLE Input (VB-Audio Virtual Cable)
   [vbcable-output] Audio output started (48 kHz, stereo, float).
   ```

---

## Fehlerbehebung

| Problem | Lösung |
|---|---|
| Log: `VB-CABLE playback endpoint 'CABLE Input' not found` | VB-CABLE fehlt oder ist deaktiviert → Schritt 1.3 / 1.4, danach OBS neu starten. |
| Keine `[vbcable-output]`-Zeilen im Log | DLL liegt nicht in `obs-plugins\64bit` oder OBS ist 32 Bit. Installer erneut ausführen. |
| Ton kommt doppelt / hallt | Alte Plugin-Version noch aktiv, OBS-Monitoring auf *CABLE Input* gestellt oder *CABLE Output* als Quelle in OBS. Siehe 4.1. |
| Eigene Lautsprecher stumm | *CABLE Input* ist Standard-Wiedergabegerät → Schritt 2.1. |
| Knacksen / Aussetzer | Abtastraten auf 48 kHz angleichen (Schritt 2.2); in der Ziel-Anwendung Audio-Verbesserungen ausschalten. |
| Ziel-Anwendung zeigt CABLE Output nicht | Anwendung/Browser komplett neu starten, nachdem VB-CABLE installiert wurde. |
| Installer: „OBS läuft noch“ | OBS auch im Infobereich beenden oder im Task-Manager `obs64.exe` beenden. |

Das Plugin öffnet *CABLE Input* einmalig beim Start von OBS. Wurde VB-CABLE erst danach installiert oder neu verbunden, OBS neu starten.

---

## Deinstallation

**Plugin:** OBS schließen → `Uninstall-AlsAdmin.cmd` doppelklicken → Ja. (Oder die DLL aus `C:\Program Files\obs-studio\obs-plugins\64bit\` löschen.)

**VB-CABLE:** Im entpackten Treiberordner `VBCABLE_Setup_x64.exe` als Administrator ausführen → **Remove Driver** → PC neu starten.
