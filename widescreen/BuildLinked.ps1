# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/widescreen/*" -Force -Recurse
dotnet publish "./widescreen.csproj" -c Release -o "$env:RELOADEDIIMODS/widescreen" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location