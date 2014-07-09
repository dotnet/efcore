$ErrorActionPreference = "Stop"

function InvokeOperation($project, $operation, $arguments) {
    $projectName = $project.ProjectName

    Write-Verbose "Using project '$projectName'"

    Write-Verbose "Build started..."
    
    $solutionBuild = $DTE.Solution.SolutionBuild
    $solutionBuild.BuildProject($solutionBuild.ActiveConfiguration.Name, $project.UniqueName, $true)
    if ($solutionBuild.LastBuildInfo) {
        throw "Build failed for project '$projectName'."
    }

    Write-Verbose "Build succeeded"

    if (![Type]::GetType('Microsoft.Data.Entity.Design.IHandler')) {
        $componentModel = Get-VSComponentModel
        $packageInstaller = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])
        $package = $packageInstaller.GetInstalledPackages() | ? Id -eq EntityFramework.Design |
            sort Version -Descending | select -First 1
        $installPath = $package.InstallPath
        $toolsPath = Join-Path $installPath tools

        Add-Type @(
            Join-Path $toolsPath IHandler.cs
            Join-Path $toolsPath Handler.cs
        )
    }

    $handler = New-Object Microsoft.Data.Entity.Design.Handler @(
        { param ($message) Write-Error $message }
        { param ($message) Write-Warning $message }
        { param ($message) Write-Host $message }
        { param ($message) Write-Verbose $message }
    )

    $outputPath = GetProperty $project.ConfigurationManager.ActiveConfiguration.Properties OutputPath
    $properties = $project.Properties
    $fullPath = GetProperty $properties FullPath
    $targetDir = Join-Path $fullPath $outputPath

    Write-Verbose "Using directory '$targetDir'"

    # TODO: Set ConfigurationFile
    $info = New-Object AppDomainSetup -Property @{
        ApplicationBase = $targetDir
        ShadowCopyFiles = 'true'
    }

    # TODO: Set DataDirectory
    $domain = [AppDomain]::CreateDomain('EntityFrameworkDesignDomain', $null, $info)
    try {
        $assemblyName = 'EntityFramework.Design'
        $typeName = 'Microsoft.Data.Entity.Design.Executor'
        $targetFileName = GetProperty $properties OutputFileName

        Write-Verbose "Using assembly '$targetFileName'"

        $executor = $domain.CreateInstanceAndUnwrap(
            $assemblyName,
            $typeName,
            $false,
            0,
            $null,
            @(@{ assemblyFileName = $targetFileName }),
            $null,
            $null)

        # TODO: Set CurrentDirectory
        $domain.CreateInstance(
            $assemblyName,
            "$typeName+$operation",
            $false,
            0,
            $null,
            ($executor, [MarshalByRefObject] $handler, $arguments),
            $null,
            $null) | Out-Null
    }
    finally {
        [AppDomain]::Unload($domain)
    }

    if ($handler.ErrorType) {
        Write-Verbose $handler.ErrorStackTrace

        throw $handler.ErrorMessage
    }
    if ($handler.HasResult) {
        return $handler.Result
    }
}

function GetProperty($properties, $propertyName) {
    $property = $properties.Item($propertyName)
    if (!$property) {
        return $null
    }

    return $property.Value
}
