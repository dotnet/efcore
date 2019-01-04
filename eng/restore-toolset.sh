function InstallDotNetSharedFramework {
  local version=$1  
  local dotnet_root=$DOTNET_INSTALL_DIR  
  if [[ ! -d "$dotnet_root/shared/Microsoft.NETCore.App/$version" ]]; then
    GetDotNetInstallScript "$dotnet_root"
    local install_script=$_GetDotNetInstallScript    
    bash "$install_script" --version $version --install-dir "$dotnet_root" --runtime "dotnet"
    local lastexitcode=$?    
    if [[ $lastexitcode != 0 ]]; then
      echo "Failed to install Shared Framework $version to '$dotnet_root' (exit code '$lastexitcode')."
      ExitWithExitCode $lastexitcode
    fi
  fi
}

InitializeDotNetCli
InstallDotNetSharedFramework "2.0.9"
