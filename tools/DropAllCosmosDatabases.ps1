$Emulator="${env:ProgramFiles}\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe"

Start-Process $Emulator -ArgumentList "/noui /shutdown" -Wait

Start-Process $Emulator -ArgumentList "/noui /resetdatapath" -Wait
if ($LASTEXITCODE -ne 0) {
	Write-Error "Emulator data path removal failed with exit code $LASTEXITCODE"
}

Start-Process $Emulator
