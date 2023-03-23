
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "volume_config/volume_config.csproj" `
              -PackageName "mrdx.audio.volume_config" `
			  -ReadmePath ./volume_config/README.md `
              -PublishOutputDir "Publish/ToUpload/volume_config" `
              -MakeDelta false -UseGitHubDelta false `
              -MetadataFileName "mrdx.audio.volume_config.ReleaseMetadata.json" `
              -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded -GitHubFallbackPattern volume_config.zip -GitHubInheritVersionFromTag false `
			  @args

./Publish.ps1 -ProjectPath "widescreen/widescreen.csproj" `
              -PackageName "mrdx.graphics.widescreen" `
			  -ReadmePath ./widescreen/README.md `
              -PublishOutputDir "Publish/ToUpload/widescreen" `
              -MakeDelta false -UseGitHubDelta false `
              -MetadataFileName "mrdx.graphics.widescreen.ReleaseMetadata.json" `
              -GitHubUserName jroweboy -GitHubRepoName mrdx_reloaded -GitHubFallbackPattern widescreen.zip -GitHubInheritVersionFromTag false `
			  @args

Pop-Location