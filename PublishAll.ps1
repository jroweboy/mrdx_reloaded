
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "MRDX.Audio.VolumeConfig/MRDX.Audio.VolumeConfig.csproj" `
              -PackageName "MRDX.Audio.VolumeConfig" `
			  -ReadmePath ./MRDX.Audio.VolumeConfig/README.md `
              -PublishOutputDir "Publish/ToUpload/MRDX.Audio.VolumeConfig" `
              -MakeDelta false -UseGitHubDelta false `
              -MetadataFileName "MRDX.Audio.VolumeConfig.ReleaseMetadata.json" `
              -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded -GitHubFallbackPattern volume_config.zip -GitHubInheritVersionFromTag false `
			  @args

./Publish.ps1 -ProjectPath "MRDX.Graphics.Widescreen/MRDX.Graphics.Widescreen.csproj" `
              -PackageName "MRDX.Graphics.Widescreen" `
			  -ReadmePath ./MRDX.Graphics.Widescreen/README.md `
              -PublishOutputDir "Publish/ToUpload/MRDX.Graphics.Widescreen" `
              -MakeDelta false -UseGitHubDelta false `
              -MetadataFileName "MRDX.Graphics.Widescreen.ReleaseMetadata.json" `
              -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded -GitHubFallbackPattern widescreen.zip -GitHubInheritVersionFromTag false `
			  @args

./Publish.ps1 -ProjectPath "MRDX.Qol.SkipDrillAnim/MRDX.Qol.SkipDrillAnim.csproj" `
              -PackageName "MRDX.Qol.SkipDrillAnim" `
			  -ReadmePath ./MRDX.Qol.SkipDrillAnim/README.md `
              -PublishOutputDir "Publish/ToUpload/MRDX.Qol.SkipDrillAnim" `
              -MakeDelta false -UseGitHubDelta false `
              -MetadataFileName "MR2DX.Qol.SkipDrillAnim.ReleaseMetadata.json" `
              -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded -GitHubFallbackPattern skip_drill.zip -GitHubInheritVersionFromTag false `
			  @args

Pop-Location
