# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/SkipDrillAnim/*" -Force -Recurse
dotnet publish "./SkipDrillAnim.csproj" -c Release -o "$env:RELOADEDIIMODS/SkipDrillAnim" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location