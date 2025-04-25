﻿
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD


# Name of interface mod, version
# These are kinda hard coded into the publish script for now.
$BaseModInterfaceVersion = "1.1.5"
$InterfaceVersions = @{
    BaseExtractDataBinInterface = "1.1.3";
    BaseModInterfaces = $BaseModInterfaceVersion
}

$AllProjects = @(
# Name of mod, version
    @("MRDX.Audio.VolumeConfig", "1.1.7", $false),
    @("MRDX.Base.ExtractDataBin", "1.1.4", $false),
    @("MRDX.Base.Mod", $BaseModInterfaceVersion, $false),
    @("MRDX.Game.HardMode", "2.1.3", $false),
    @("MRDX.Game.DynamicTournaments", "0.2.4", $false),
#    Not ready for release yet, so leave it commented out for now
#    @("MRDX.Game.MonsterEditor", "1.0.0", $false),
    @("MRDX.Graphics.Widescreen", "1.2.4", $false),
    @("MRDX.Qol.BattleTimer", "1.0.4", $false),
    @("MRDX.Qol.FastForward", "1.3.6", $false),
    @("MRDX.Qol.MagicBananaStatic", "0.1.0", $false),
    @("MRDX.Qol.SkipDrillAnim", "1.3.2", $false),
    @("MRDX.Qol.TurboInput", "1.0.6", $false),
    @("MRDX.Ui.RawTechValues", "1.0.9", $false)
)

Write-Output "Starting Publish All"

foreach ($Project in $AllProjects)
{
    $ProjectName = $Project[0]
    $ProjectVersion = $Project[1]
    $ProjectUseDelta = $Project[2]
    (Get-Content "$ProjectName/ModConfig.template.json").replace("{{ MOD_VERSION }}", "$ProjectVersion") | Set-Content "$ProjectName/ModConfig.json"
    Write-Output "Publishing $ProjectName"
    ./Publish.ps1 -ProjectPath "$ProjectName/$ProjectName.csproj" `
          -PackageName "$ProjectName" `
          -ReadmePath "./$ProjectName/README.md" `
          -Version "$ProjectVersion" `
          @InterfaceVersions `
          -PublishOutputDir "Publish/ToUpload/$ProjectName" `
          -MakeDelta $ProjectUseDelta -UseGitHubDelta $ProjectUseDelta `
          -MetadataFileName "$ProjectName.ReleaseMetadata.json" `
          -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded `
          -GitHubFallbackPattern mod.zip -GitHubInheritVersionFromTag false `
          @args
}

Write-Output "Publishing Complete"

Pop-Location
