# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/volume_config/*" -Force -Recurse
dotnet publish "./volume_config.csproj" -c Release -o "$env:RELOADEDIIMODS/volume_config" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location