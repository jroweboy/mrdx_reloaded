
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD


$InterfaceVersions = @{
    BaseExtractDataBinInterface = "1.0.1";
    BaseModInterfaces = "1.0.1"
}

$AllProjects = @(
# Name of mod, version, shouldPublish
    @("MRDX.Audio.VolumeConfig", "1.1.1"),
    @("MRDX.Base.ExtractDataBin", "1.0.1"),
    @("MRDX.Base.Mod", "1.0.1"),
    @("MRDX.Game.HardMode", "1.0.1"),
#    Not ready for release yet, so leave it as publish false for now
#    @("MRDX.Game.MonsterEditor", "1.0.0"),
    @("MRDX.Graphics.Widescreen", "1.1.1"),
    @("MRDX.Qol.FastForward", "1.2.1"),
    @("MRDX.Qol.SkipDrillAnim", "1.2.1")
)

Write-Output "Starting Publish All"

foreach ($Project in $AllProjects)
{
    $ProjectName = $Project[0]
    $ProjectVersion = $Project[1]
    Write-Output "Publishing $ProjectName"
    ./Publish.ps1 -ProjectPath "$ProjectName/$ProjectName.csproj" `
          -PackageName "$ProjectName" `
          -ReadmePath "./$ProjectName/README.md" `
          -Version "$ProjectVersion" `
          @InterfaceVersions `
          -PublishOutputDir "Publish/ToUpload/$ProjectName" `
          -MakeDelta true -UseGitHubDelta true `
          -MetadataFileName "$ProjectName.ReleaseMetadata.json" `
          -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded `
          -GitHubFallbackPattern mod.zip -GitHubInheritVersionFromTag false `
          @args
}

Write-Output "Publishing Complete"

Pop-Location
