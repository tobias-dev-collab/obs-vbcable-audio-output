#Requires -RunAsAdministrator
<#
  Installiert obs-vbcable-audio-output.dll in den OBS-Plugin-Ordner.
  - findet den OBS-Installationsordner über die Registry (Fallback: C:\Program Files\obs-studio)
  - sichert eine vorhandene Version als .bak
  - deaktiviert die alte Vorgängerversion des Plugins, damit der Ton nicht doppelt ausgegeben wird
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$pluginName = 'obs-vbcable-audio-output.dll'
$legacyNames = @('joyclub-obs-audio-bridge-plugin.dll')
$packageDirectory = Split-Path -Parent $PSCommandPath
$pluginFile = Join-Path $packageDirectory $pluginName

function Get-ObsDirectory {
    foreach ($key in 'HKLM:\SOFTWARE\OBS Studio', 'HKLM:\SOFTWARE\WOW6432Node\OBS Studio') {
        try {
            $value = (Get-ItemProperty -LiteralPath $key -ErrorAction Stop).'(default)'
            if ($value -and (Test-Path -LiteralPath $value -PathType Container)) { return $value }
        } catch { }
    }
    return 'C:\Program Files\obs-studio'
}

if (-not (Test-Path -LiteralPath $pluginFile -PathType Leaf)) {
    throw "Plugin-Datei nicht gefunden: $pluginFile"
}

$obsDirectory = Get-ObsDirectory
$pluginDirectory = Join-Path $obsDirectory 'obs-plugins\64bit'
if (-not (Test-Path -LiteralPath $pluginDirectory -PathType Container)) {
    throw "OBS-Plugin-Ordner nicht gefunden: $pluginDirectory  (Ist OBS Studio 64 Bit installiert?)"
}

if (Get-Process -Name 'obs64' -ErrorAction SilentlyContinue) {
    throw 'OBS läuft noch. Bitte OBS vollständig schließen (auch im Infobereich) und den Installer erneut starten.'
}

foreach ($legacy in $legacyNames) {
    $legacyFile = Join-Path $pluginDirectory $legacy
    if (Test-Path -LiteralPath $legacyFile -PathType Leaf) {
        Move-Item -LiteralPath $legacyFile -Destination "$legacyFile.disabled" -Force
        Write-Host "Alte Plugin-Version deaktiviert: $legacyFile.disabled"
    }
}

$targetFile = Join-Path $pluginDirectory $pluginName
if (Test-Path -LiteralPath $targetFile -PathType Leaf) {
    Copy-Item -LiteralPath $targetFile -Destination "$targetFile.bak" -Force
    Write-Host "Vorhandene Version gesichert: $targetFile.bak"
}

Copy-Item -LiteralPath $pluginFile -Destination $targetFile -Force
Unblock-File -LiteralPath $targetFile -ErrorAction SilentlyContinue

$cable = Get-CimInstance -ClassName Win32_SoundDevice -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*VB-Audio*' }
Write-Host ''
Write-Host "Plugin installiert: $targetFile" -ForegroundColor Green
if (-not $cable) {
    Write-Warning 'VB-Audio Virtual Cable wurde nicht gefunden. Bitte zuerst VB-CABLE installieren (siehe INSTALLATION.md).'
}
Write-Host 'OBS jetzt starten und in der Ziel-Anwendung "CABLE Output (VB-Audio Virtual Cable)" als Mikrofon wählen.'
