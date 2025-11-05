# Starts the Azure Cosmos DB Emulator in a Windows Docker container and waits for it to be ready.
# Tests run on the host machine connecting to the emulator via https://localhost:8081.
# Usage: .\run-cosmos-container.ps1
param()

$ErrorActionPreference = 'Stop'
$image = 'mcr.microsoft.com/cosmosdb/windows/azure-cosmos-emulator'
$containerName = 'cosmos-emulator'
$port = 8081
$maxRetries = 90
$retryDelaySec = 2

Write-Host "Pulling image: $image"
docker pull $image
if ($LASTEXITCODE -ne 0) { throw "docker pull failed with exit code $LASTEXITCODE" }

# -t is required because Start.ps1 sets [Console]::BufferWidth which needs a TTY handle.
Write-Host "Starting Cosmos DB Emulator container on port $port..."
docker run -d -t `
    --name $containerName `
    --publish "${port}:8081" `
    --memory 2G `
    $image
if ($LASTEXITCODE -ne 0) { throw "docker run failed with exit code $LASTEXITCODE" }

Write-Host "Waiting for emulator to be ready (up to $($maxRetries * $retryDelaySec)s)..."
$ready = $false
for ($i = 0; $i -lt $maxRetries; $i++) {
    Start-Sleep -Seconds $retryDelaySec
    try {
        # Any HTTP response (even 401) means the emulator is up and accepting connections.
        $null = Invoke-WebRequest -Uri "https://localhost:${port}/" -UseBasicParsing -TimeoutSec 5
        $ready = $true
    } catch [Microsoft.PowerShell.Commands.HttpResponseException] {
        # Got an HTTP error response (401, 404, etc.) — emulator is reachable.
        $ready = $true
    } catch {
        Write-Host "  Attempt $($i+1)/$maxRetries - not ready yet..."
    }
    if ($ready) {
        Write-Host "Cosmos DB Emulator is ready on port $port."
        break
    }
}

if (-not $ready) {
    Write-Host "Emulator did not become ready. Container logs:"
    docker logs $containerName
    docker stop $containerName 2>$null
    docker rm -f $containerName 2>$null
    exit 1
}

exit 0
