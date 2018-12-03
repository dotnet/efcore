function InstallDotNetSharedFramework([string]$version) {  
  $dotnetRoot = $env:DOTNET_INSTALL_DIR
  if (-not (Test-Path(Join-Path $dotnetRoot "shared\Microsoft.NETCore.App\$version"))) {
    $installScript = GetDotNetInstallScript $dotnetRoot
    & $installScript -Version $version -InstallDir $dotnetRoot -Runtime "dotnet"
    if($lastExitCode -ne 0) {
      throw "Failed to install shared Framework $version to '$dotnetRoot' (exit code '$lastExitCode')."
    }
  }
}

InitializeDotnetCli
InstallDotNetSharedFramework "2.0.9"
