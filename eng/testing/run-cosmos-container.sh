#!/usr/bin/env bash
# Starts the Azure Cosmos DB Linux (vNext) Emulator in a Docker container and waits for it to be ready.
# Tests run on the host machine connecting to the emulator via https://localhost:8081.
# The --protocol https flag is required because the .NET SDK does not support HTTP mode.
# Usage: ./run-cosmos-container.sh

set -e

image='mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview'
container_name='cosmos-emulator'
port=8081
max_retries=30
retry_delay=2

echo "Pulling image: $image"
docker pull "$image"

if docker ps -a --format '{{.Names}}' | grep -Eq "^${container_name}\$"; then
    echo "Removing existing Cosmos DB Emulator container: $container_name"
    docker rm -f "$container_name"
fi

echo "Starting Cosmos DB Emulator container on port $port with HTTPS..."
docker run -d \
    --name "$container_name" \
    --publish "${port}:8081" \
    "$image" \
    --protocol https \
    --enable-explorer false

echo "Waiting for emulator to be ready (up to ~$((max_retries * retry_delay))s)..."
ready=false
for i in $(seq 1 "$max_retries"); do
    sleep "$retry_delay"
    if curl -ks --connect-timeout "$retry_delay" --max-time "$retry_delay" "https://localhost:${port}/" -o /dev/null; then
        ready=true
        echo "Cosmos DB Emulator is ready."
        break
    fi
    echo "  Attempt $i/$max_retries - not ready yet..."
done

if [ "$ready" != true ]; then
    echo "Emulator did not become ready. Container logs:"
    docker logs "$container_name"
    docker stop "$container_name" 2>/dev/null || true
    docker rm -f "$container_name" 2>/dev/null || true
    exit 1
fi

exit 0
