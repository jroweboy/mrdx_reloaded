# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/MRDX.Base.ExtractDataBin/*" -Force -Recurse
dotnet publish "./MRDX.Base.ExtractDataBin.csproj" -c Release -o "$env:RELOADEDIIMODS/MRDX.Base.ExtractDataBin" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location