$ErrorActionPreference = "Stop"

$EFDefaultParameterValues = @{
    ProjectName = ''
    ContextTypeName = ''
}

#
# Use-DbContext
#

Register-TabExpansion Use-DbContext @{
    Context = { param ($context) GetContextTypes $context.Project }
    Project = { GetProjects }
}

function Use-DbContext {
    [CmdletBinding()]
    param ([Parameter(Mandatory = $true)] [string] $Context, [string] $Project)

    $dteProject = GetProject $Project
    $contextTypeName = InvokeOperation $dteProject GetContextType @{ name = $Context }

    $EFDefaultParameterValues.ContextTypeName = $contextTypeName
    $EFDefaultParameterValues.ProjectName = $dteProject.ProjectName
}

#
# Add-Migration
#

Register-TabExpansion Add-Migration @{
    Context = { param ($context) GetContextTypes $context.Project }
    Project = { GetProjects }
}

function Add-Migration {
    [CmdletBinding()]
    param ([Parameter(Mandatory = $true)] [string] $Name, [string] $Context, [string] $Project)

    $values = ProcessCommonParameters $Context $Project
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $artifacts = InvokeOperation $dteProject AddMigration @{
        migrationName = $Name
        contextTypeName = $contextTypeName
    }

    $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole
}

#
# Apply-Migration
#

Register-TabExpansion Apply-Migration @{
    Migration = { param ($context) GetMigrations $context.Context $context.Project }
    Context = { param ($context) GetContextTypes $context.Project }
    Project = { GetProjects }
}

# TODO: WhatIf
function Apply-Migration {
    [CmdletBinding()]
    param ([string] $Migration, [string] $Context, [string] $Project)

    $values = ProcessCommonParameters $Context $Project
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $targetFrameworkMoniker = GetProperty $dteProject.Properties TargetFrameworkMoniker
    $frameworkName = New-Object System.Runtime.Versioning.FrameworkName $targetFrameworkMoniker
    if ($frameworkName.Identifier -in '.NETCore', 'WindowsPhoneApp') {
        throw 'Apply-Migration should not be used with Phone/Store apps. Instead, call DbContext.Database.AsMigrationsEnabled().ApplyMigrations() at runtime.'
    }

    InvokeOperation $dteProject ApplyMigration @{
        migrationName = $Migration
        contextTypeName = $contextTypeName
    }
}

#
# Update-Database (Obsolete)
#

Register-TabExpansion Update-Database @{
    Migration = { param ($context) GetMigrations $context.Context $context.Project }
    Context = { param ($context) GetContextTypes $context.Project }
    Project = { GetProjects }
}

function Update-Database {
    [CmdletBinding()]
    param ([string] $Migration, [string] $Context, [string] $Project)

    Write-Warning 'Update-Database is obsolete. Use Apply-Migration instead.'

    Apply-Migration $Migration -Context $Context -Project $Project
}

#
# Script-Migration
#

Register-TabExpansion Script-Migration @{
    From = { param ($context) GetMigrations $context.Context $context.Project }
    To = { param ($context) GetMigrations $context.Context $context.Project }
    Context = { param ($context) GetContextTypes $context.Project }
    Project = { GetProjects }
}

function Script-Migration {
    [CmdletBinding()]
    param ([string] $From, [string] $To, [switch] $Idempotent, [string] $Context, [string] $Project)

    $values = ProcessCommonParameters $Context $Project
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $script = InvokeOperation $dteProject ScriptMigration @{
        fromMigrationName = $From
        toMigrationName = $To
        idempotent = [bool]$Idempotent
        contextTypeName = $contextTypeName
    }

    try {
        # NOTE: Certain SKUs cannot create new SQL files
        $window = $DTE.ItemOperations.NewFile('General\Sql File')
        $textDocument = $window.Document.Object('TextDocument')
        $editPoint = $textDocument.StartPoint.CreateEditPoint()
        $editPoint.Insert($script)
    }
    catch {
        $fullPath = GetProperty $dteProject.Properties FullPath
        $intermediatePath = GetProperty $dteProject.ConfigurationManager.ActiveConfiguration.Properties IntermediatePath
        $fullIntermediatePath = Join-Path $fullPath $intermediatePath
        $fileName = [IO.Path]::GetRandomFileName()
        $fileName = [IO.Path]::ChangeExtension($fileName, '.sql')
        $scriptFile = Join-Path $fullIntermediatePath $fileName
        $script | Out-File $scriptFile
        $DTE.ItemOperations.OpenFile($scriptFile) | Out-Null
    }

    ShowConsole
}

#
# Remove-Migration
#

Register-TabExpansion Remove-Migration @{
    Context = { param ($context) GetContextTypes $context.Project }
    Project = { GetProjects }
}

function Remove-Migration {
    [CmdletBinding()]
    param ([string] $Context, [string] $Project)

    $values = ProcessCommonParameters $Context $Project
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $filesToDelete = InvokeOperation $dteProject RemoveMigration @{ contextTypeName = $contextTypeName }

	$filesToDelete | ?{ Test-Path $_ } | %{ (GetProjectItem $dteProject $_).Delete() }
}

#
# (Private Helpers)
#

function GetProjects {
    $projects = Get-Project -All
    $groups = $projects | group Name

    return $projects | %{
        if ($groups | ? Name -eq $_.Name | ? Count -eq 1) {
            return $_.Name
        }

        return $_.ProjectName
    }
}

function GetContextTypes($projectName) {
    $project = GetProject $projectName

    $contextTypes = InvokeOperation $project GetContextTypes -skipBuild

    return $contextTypes | %{ $_.SafeName }
}

function GetMigrations($contextTypeName, $projectName) {
    $values = ProcessCommonParameters $contextTypeName $projectName
    $project = $values.Project
    $contextTypeName = $values.ContextTypeName

    $migrations = InvokeOperation $project GetMigrations @{ contextTypeName = $contextTypeName } -skipBuild

    return $migrations | %{ $_.SafeName }
}

function ProcessCommonParameters($contextTypeName, $projectName) {
    $project = GetProject $projectName

    if (!$contextTypeName -and $project.ProjectName -eq $EFDefaultParameterValues.ProjectName) {
        $contextTypeName = $EFDefaultParameterValues.ContextTypeName
    }

    return @{
        Project = $project
        ContextTypeName = $contextTypeName
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

        Write-Verbose "Build succeeded."
    }

    if (![Type]::GetType('Microsoft.Data.Entity.Commands.ILogHandler')) {
        $componentModel = Get-VSComponentModel
        $packageInstaller = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerServices])
        $package = $packageInstaller.GetInstalledPackages() | ? Id -eq EntityFramework.Commands |
            sort Version -Descending | select -First 1
        $installPath = $package.InstallPath
        $toolsPath = Join-Path $installPath tools

        Add-Type @(
            Join-Path $toolsPath IHandlers.cs
            Join-Path $toolsPath Handlers.cs
        )
    }

    $logHandler = New-Object Microsoft.Data.Entity.Commands.LogHandler @(
        { param ($message) Write-Warning $message }
        { param ($message) Write-Host $message }
        { param ($message) Write-Verbose $message }
    )

    $outputPath = GetProperty $project.ConfigurationManager.ActiveConfiguration.Properties OutputPath
    $properties = $project.Properties
    $fullPath = GetProperty $properties FullPath
    $targetDir = Join-Path $fullPath $outputPath

    Write-Verbose "Using directory '$targetDir'."

    # TODO: Set ConfigurationFile
    $info = New-Object AppDomainSetup -Property @{
        ApplicationBase = $targetDir
        ShadowCopyFiles = 'true'
    }

    # TODO: Set DataDirectory
    $domain = [AppDomain]::CreateDomain('EntityFrameworkDesignDomain', $null, $info)
    try {
        $assemblyName = 'EntityFramework.Commands'
        $typeName = 'Microsoft.Data.Entity.Commands.Executor'
        $targetFileName = GetProperty $properties OutputFileName
        $targetPath = Join-Path $targetDir $targetFileName
        $rootNamespace = GetProperty $properties RootNamespace

        Write-Verbose "Using assembly '$targetFileName'."

        $executor = $domain.CreateInstanceAndUnwrap(
            $assemblyName,
            $typeName,
            $false,
            0,
            $null,
            @(
                [MarshalByRefObject]$logHandler,
                @{
                    targetPath = [string]$targetPath
                    projectDir = $fullPath
                    rootNamespace = $rootNamespace
                }
            ),
            $null,
            $null)

        $resultHandler = New-Object Microsoft.Data.Entity.Commands.ResultHandler
        $currentDirectory = [IO.Directory]::GetCurrentDirectory()

        [IO.Directory]::SetCurrentDirectory($targetDir)
        try {
            $domain.CreateInstance(
                $assemblyName,
                "$typeName+$operation",
                $false,
                0,
                $null,
                ($executor, [MarshalByRefObject]$resultHandler, $arguments),
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

    if ($resultHandler.ErrorType) {
        Write-Verbose $resultHandler.ErrorStackTrace

        throw $resultHandler.ErrorMessage
    }
    if ($resultHandler.HasResult) {
        return $resultHandler.Result
    }
}

function GetProperty($properties, $propertyName) {
    $property = $properties.Item($propertyName)
    if (!$property) {
        return $null
    }

    return $property.Value
}

function GetProjectItem($project, $path) {
	$fullPath = GetProperty $project.Properties FullPath
	$itemDirectory = (Split-Path $path.Substring($fullPath.Length) -Parent)

	$projectItems = $project.ProjectItems
	if ($itemDirectory) {
		$directories = $itemDirectory.Split('\')
		$directories | %{
            $projectItems = $projectItems.Item($_).ProjectItems
        }
	}

	$itemName = Split-Path $path -Leaf

	return $projectItems.Item($itemName)
}
