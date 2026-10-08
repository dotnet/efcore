@echo off
setlocal enabledelayedexpansion

set RETRIES=0
set MAX_RETRIES=60

:check
if !RETRIES! geq %MAX_RETRIES% (
    echo ERROR: Cosmos DB Emulator did not start after %MAX_RETRIES% attempts.
    exit /b 1
)

for /f %%a in ('curl -k -s -o nul -w "%%{http_code}" https://localhost:8081/ 2^>nul') do set HTTP_CODE=%%a

if "!HTTP_CODE!"=="401" (
    echo Cosmos DB Emulator is running ^(HTTP 401 received^).
    timeout /t 2 >nul
    exit /b 0
)

set /a RETRIES+=1
echo Attempt !RETRIES!/%MAX_RETRIES%: Waiting for Cosmos DB Emulator ^(got HTTP !HTTP_CODE!^)...
timeout /t 5 >nul
goto :check
