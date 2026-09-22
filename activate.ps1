#
# To exit from the environment this creates, execute the 'deactivate' function.
#

if ($MyInvocation.InvocationName -ne '.') {
    Write-Host -f Red "This script must be dot sourced. Run it by invoking '. .\activate.ps1'."
    return
}

function deactivate ([switch]$init) {
    # reset old environment variables
    if (Test-Path variable:_OLD_PATH) {
        $env:PATH = $_OLD_PATH
        Remove-Item variable:_OLD_PATH
    }

    if (test-path function:_old_prompt) {
        Set-Item Function:prompt -Value $function:_old_prompt -ErrorAction Ignore
        Remove-Item function:_old_prompt
    }

    Remove-Item env:DOTNET_ROOT -ErrorAction Ignore
    Remove-Item env:DOTNET_MULTILEVEL_LOOKUP -ErrorAction Ignore
    if (-not $init) {
        # Remove the deactivate function
        Remove-Item function:deactivate
    }
}

# Cleanup the environment
deactivate -init

$_OLD_PATH = $env:PATH
# Tell dotnet where to find itself
$env:DOTNET_ROOT = "$PSScriptRoot\.dotnet"
# Tell dotnet not to look beyond the DOTNET_ROOT folder for more dotnet things
$env:DOTNET_MULTILEVEL_LOOKUP = 0
# Put dotnet first on PATH
$env:PATH = "${env:DOTNET_ROOT};${env:PATH}"

# Set the shell prompt
if (-not $env:DISABLE_CUSTOM_PROMPT) {
    $function:_old_prompt = $function:prompt
    function dotnet_prompt {
        # Add a prefix to the current prompt, but don't discard it.
        Write-Host -f Green "($(Split-Path $PSScriptRoot -Leaf)) " -NoNewLine
        & $function:_old_prompt
    }

    Set-Item Function:prompt -Value $function:dotnet_prompt -ErrorAction Ignore
}

Write-Host -f Magenta "Enabled the .NET Core environment. Execute 'deactivate' to exit."
if (-not (Test-Path "${env:DOTNET_ROOT}\dotnet.exe")) {
    Write-Host -f Yellow ".NET Core has not been installed yet. Run $PSScriptRoot\restore.cmd to install it."
}
else {
    Write-Host "dotnet = ${env:DOTNET_ROOT}\dotnet.exe"
}
