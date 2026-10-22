#Requires -RunAsAdministrator
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$pluginName = 'obs-vbcable-audio-output.dll'

$obsDirectory = 'C:\Program Files\obs-studio'
foreach ($key in 'HKLM:\SOFTWARE\OBS Studio', 'HKLM:\SOFTWARE\WOW6432Node\OBS Studio') {
    try {
        $value = (Get-ItemProperty -LiteralPath $key -ErrorAction Stop).'(default)'
        if ($value -and (Test-Path -LiteralPath $value -PathType Container)) { $obsDirectory = $value; break }
    } catch { }
}

if (Get-Process -Name 'obs64' -ErrorAction SilentlyContinue) {
    throw 'OBS läuft noch. Bitte OBS vollständig schließen und erneut starten.'
}

$targetFile = Join-Path $obsDirectory "obs-plugins\64bit\$pluginName"
foreach ($file in $targetFile, "$targetFile.bak") {
    if (Test-Path -LiteralPath $file -PathType Leaf) {
        Remove-Item -LiteralPath $file -Force
        Write-Host "Entfernt: $file"
    }
}
Write-Host 'Plugin deinstalliert.' -ForegroundColor Green
