param(
    [string]$ValheimPath = "D:\SteamLibrary\steamapps\common\Valheim",
    [string]$DeployProfile = "$env:USERPROFILE\AppData\Roaming\com.kesomannen.gale\valheim\profiles\New Release",
    [string]$BepInExPath = "",
    [switch]$Deploy,
    [switch]$Package
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root "src\LetsGo\LetsGo.csproj"
$dll = Join-Path $root "artifacts\LetsGo.dll"
$thunderstore = Join-Path $root "thunderstore"
$manifest = Get-Content (Join-Path $thunderstore "manifest.json") | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($BepInExPath)) {
    $BepInExPath = Join-Path $DeployProfile "BepInEx"
}

$buildArgs = @(
    $project,
    "-c", "Release",
    "-p:ValheimPath=$ValheimPath",
    "-p:BepInExPath=$BepInExPath",
    "-p:GaleProfile=$DeployProfile"
)

dotnet build @buildArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Built: $dll"
Write-Host "Refs: ValheimManaged=$ValheimPath\valheim_Data\Managed"
Write-Host "Refs: BepInExCore=$BepInExPath\core"

if ($Deploy) {
    $pluginDir = Join-Path $DeployProfile "BepInEx\plugins\Hardwire99-LetsGo"
    New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
    $dest = Join-Path $pluginDir "LetsGo.dll"

    try {
        Copy-Item $dll $dest -Force
        Copy-Item (Join-Path $thunderstore "manifest.json") (Join-Path $pluginDir "manifest.json") -Force
        Copy-Item (Join-Path $thunderstore "CHANGELOG.md") (Join-Path $pluginDir "CHANGELOG.md") -Force
        Copy-Item (Join-Path $thunderstore "README.md") (Join-Path $pluginDir "README.md") -Force
        Write-Host "Deployed to $dest"
    }
    catch {
        $pending = Join-Path $pluginDir "LetsGo.dll.pending"
        Copy-Item $dll $pending -Force
        Write-Warning "Valheim has the plugin locked. Close the game, then replace LetsGo.dll with LetsGo.dll.pending"
        Write-Host "Built update saved to $pending"
    }
}

if ($Package) {
    $staging = Join-Path $root "artifacts\thunderstore-staging"
    $packageName = "{0}-{1}.zip" -f $manifest.name, $manifest.version_number
    $packagePath = Join-Path (Join-Path $root "artifacts") $packageName
    $icon = Join-Path $thunderstore "icon.png"

    if (Test-Path $staging) {
        Remove-Item $staging -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $staging | Out-Null

    Copy-Item $dll (Join-Path $staging "LetsGo.dll") -Force
    Copy-Item (Join-Path $thunderstore "manifest.json") (Join-Path $staging "manifest.json") -Force
    Copy-Item (Join-Path $thunderstore "README.md") (Join-Path $staging "README.md") -Force
    Copy-Item (Join-Path $thunderstore "CHANGELOG.md") (Join-Path $staging "CHANGELOG.md") -Force
    if (Test-Path $icon) {
        Copy-Item $icon (Join-Path $staging "icon.png") -Force
    }
    else {
        Write-Warning "thunderstore/icon.png is missing; zip will not be Thunderstore-valid until you add a 256x256 icon."
    }

    if (Test-Path $packagePath) {
        Remove-Item $packagePath -Force
    }
    Compress-Archive -Path (Join-Path $staging "*") -DestinationPath $packagePath -Force
    Write-Host "Packaged: $packagePath"
}

if (-not $Deploy -and -not $Package) {
    Write-Host "Skipped deploy/package. Pass -Deploy and/or -Package as needed."
}
