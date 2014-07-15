$ErrorActionPreference = "Stop"

$EFDefaultParameterValues = @{
    ProjectName = ''
    ContextName = ''
}

#
# Use-EFContext
#

Register-TabExpansion Use-DbContext @{
    ContextName = { param ($context) GetContextNames $context.ProjectName }
    ProjectName = { GetProjectNames }
}

function Use-DbContext {
    [CmdletBinding()]
    param ([Parameter(Mandatory = $true)] [string] $ContextName, [string] $ProjectName)

    $project = GetProject $ProjectName
    $ProjectName = $project.ProjectName
    $ContextName = InvokeOperation $project GetContextName @{ contextName = $ContextName }

    $EFDefaultParameterValues.ContextName = $ContextName
    $EFDefaultParameterValues.ProjectName = $ProjectName
}

#
# Add-Migration
#

Register-TabExpansion Add-Migration @{
    ContextName = { param ($context) GetContextNames $context.ProjectName }
    ProjectName = { GetProjectNames }
}

function Add-Migration {
    [CmdletBinding()]
    param ([Parameter(Mandatory = $true)] [string] $MigrationName, [string] $ContextName, [string] $ProjectName)

    $values = ProcessCommonParameters $ContextName $ProjectName
    $project = $values.Project
    $ContextName = $values.ContextName

    $artifacts = InvokeOperation $project CreateMigration @{
        migrationName = $MigrationName
        contextName = $ContextName
    }

    $artifacts | %{ $project.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole
}

#
# Update-Database
#

Register-TabExpansion Update-Database @{
    MigrationName = { param ($context) GetMigrationNames $context.ContextName $context.ProjectName }
    ContextName = { param ($context) GetContextNames $context.ProjectName }
    ProjectName = { Get-ProjectNames }
}

# TODO: WhatIf
function Update-Database {
    [CmdletBinding()]
    param ([string] $MigrationName, [string] $ContextName, [string] $ProjectName)

    $values = ProcessCommonParameters $ContextName $ProjectName
    $project = $values.Project
    $ContextName = $values.ContextName

    InvokeOperation $project PublishMigration @{
        migrationName = $MigrationName
        contextName = $ContextName
    }
}

#
# Script-Migration
#

Register-TabExpansion Script-Migration @{
    FromMigration = { param ($context) GetMigrationNames $context.ContextName $context.ProjectName }
    ToMigration = { param ($context) GetMigrationNames $context.ContextName $context.ProjectName }
    ContextName = { param ($context) GetContextNames $context.ProjectName }
    ProjectName = { Get-ProjectNames }
}

function Script-Migration {
    [CmdletBinding()]
    param (
        [string] $FromMigration,
        [string] $ToMigration,
        [switch] $Idempotent,
        [string] $ContextName,
        [string] $ProjectName
    )

    $values = ProcessCommonParameters $ContextName $ProjectName
    $project = $values.Project
    $ContextName = $values.ContextName

    $script = InvokeOperation $project CreateMigrationScript @{
        fromMigration = $FromMigration
        toMigration = $ToMigration
        idempotent = [bool]$Idempotent
        contextName = $ContextName
    }

    # TODO: New SQL File
    $window = $DTE.ItemOperations.NewFile('General\Text File')
    $textDocument = $window.Document.Object('TextDocument')
    $editPoint = $textDocument.StartPoint.CreateEditPoint();
    $editPoint.Insert($script);
    ShowConsole
}

#
# (Private Helpers)
#

function GetProjectNames {
    $projects = Get-Project -All
    $groups = $projects | group Name

    return $projects | %{
        if ($groups | ? Name -eq $_.Name | ? Count -eq 1) {
            return $_.Name
        }

        return $_.ProjectName
    }
}

function GetContextNames($projectName) {
    $project = GetProject $projectName

    $contextNames = InvokeOperation $project GetContextNames -skipBuild

    return $contextNames | %{ $_.SafeName }
}

function GetMigrationNames($contextName, $projectName) {
    $values = ProcessCommonParameters $contextName $projectName
    $project = $values.Project
    $contextName = $values.ContextName

    $migrationNames = InvokeOperation $project GetMigrationNames @{ contextName = $contextName } -skipBuild

    return $migrationNames | %{ $_.SafeName }
}

function ProcessCommonParameters($contextName, $projectName) {
    if (!$contextName) {
        $contextName = $EFDefaultParameterValues.ContextName
        $projectName = $EFDefaultParameterValues.ProjectName
    }

    if (!$projectName) {
        $projectName = $EFDefaultParameterValues.ProjectName
    }

    $project = GetProject $projectName

    return @{
        Project = $project
        ContextName = $contextName
    }
}

function GetProject($projectName) {
    if ($projectName) {
        return Get-Project $projectName
    }

    return Get-Project
}

function ShowConsole {
    $componentModel = Get-VSComponentModel
    $powerConsoleWindow = $componentModel.GetService([NuGetConsole.IPowerConsoleWindow])
    $powerConsoleWindow.Show()
}

function InvokeOperation($project, $operation, $arguments = @{}, [switch] $skipBuild) {
    $projectName = $project.ProjectName

    Write-Verbose "Using project '$projectName'"

    if (!$skipBuild) {
        Write-Verbose "Build started..."

        $solutionBuild = $DTE.Solution.SolutionBuild
        $solutionBuild.BuildProject($solutionBuild.ActiveConfiguration.Name, $project.UniqueName, $true)
        if ($solutionBuild.LastBuildInfo) {
            throw "Build failed for project '$projectName'."
        }

        Write-Verbose "Build succeeded"
    }

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
        $rootNamespace = GetProperty $properties RootNamespace

        Write-Verbose "Using assembly '$targetFileName'"

        $executor = $domain.CreateInstanceAndUnwrap(
            $assemblyName,
            $typeName,
            $false,
            0,
            $null,
            @(
                @{
                    targetDir = [string]$targetDir
                    targetFileName = $targetFileName
                    projectDir = $fullPath
                    rootNamespace = $rootNamespace
                }
            ),
            $null,
            $null)

        $currentDirectory = [IO.Directory]::GetCurrentDirectory()

        [IO.Directory]::SetCurrentDirectory($targetDir)
        try {
            $domain.CreateInstance(
                $assemblyName,
                "$typeName+$operation",
                $false,
                0,
                $null,
                ($executor, [MarshalByRefObject]$handler, $arguments),
                $null,
                $null) | Out-Null
        }
        finally {
            [IO.Directory]::SetCurrentDirectory($currentDirectory)
        }
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
