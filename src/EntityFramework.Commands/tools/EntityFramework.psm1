$ErrorActionPreference = 'Stop'

$EFDefaultParameterValues = @{
    ProjectName = ''
    ContextTypeName = ''
}

#
# Use-DbContext
#

Register-TabExpansion Use-DbContext @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project }
    Project = { GetProjects }
}

<#
.SYNOPSIS
    Sets the default DbContext to use.

.DESCRIPTION
    Sets the default DbContext to use.

.PARAMETER Context
    Specifies the DbContext to use.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.LINK
    about_EntityFramework
#>
function Use-DbContext {
    [CmdletBinding(PositionalBinding = $false)]
    param ([Parameter(Position = 0, Mandatory = $true)] [string] $Context, [string] $Project)

    $dteProject = GetProject $Project
    $contextTypeName = InvokeOperation $dteProject GetContextType @{ name = $Context }

    $EFDefaultParameterValues.ContextTypeName = $contextTypeName
    $EFDefaultParameterValues.ProjectName = $dteProject.ProjectName
}

#
# Add-Migration
#

Register-TabExpansion Add-Migration @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Adds a new migration.

.DESCRIPTION
    Adds a new migration.

.PARAMETER Name
    Specifies the name of the migration.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the start-up project to use. If omitted, the solution's start-up project is used.

.LINK
    Remove-Migration
    Update-Database
    about_EntityFramework
#>
function Add-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Name,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject)

    $values = ProcessCommonParameters $Context $Project $StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName
    $dteStartupProject = $values.StartupProject

    $artifacts = InvokeOperation $dteProject AddMigration @{
        migrationName = $Name
        contextTypeName = $contextTypeName
    } -startupProject $dteStartupProject

    $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole

    Write-Host 'To undo this action, use Remove-Migration.'
}

#
# Update-Database
#

Register-TabExpansion Update-Database @{
    Migration = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project }
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Updates the database to a specified migration.

.DESCRIPTION
    Updates the database to a specified migration.

.PARAMETER Migration
    Specifies the target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the start-up project to use. If omitted, the solution's start-up project is used.

.LINK
    Script-Migration
    about_EntityFramework
#>
function Update-Database {
    [CmdletBinding(PositionalBinding = $false)]
    param ([Parameter(Position = 0)] [string] $Migration, [string] $Context, [string] $Project, [string] $StartupProject)

    $values = ProcessCommonParameters $Context $Project $StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName
    $dteStartupProject = $values.StartupProject

    $targetFrameworkMoniker = GetProperty $dteProject.Properties TargetFrameworkMoniker
    $frameworkName = New-Object System.Runtime.Versioning.FrameworkName $targetFrameworkMoniker
    if ($frameworkName.Identifier -eq '.NETCore') {
        throw 'Update-Database should not be used with Universal Windows apps. Instead, call DbContext.Database.ApplyMigrations() at runtime.'
    }

    InvokeOperation $dteProject ApplyMigration @{
        migrationName = $Migration
        contextTypeName = $contextTypeName
    } -startupProject $dteStartupProject
}

#
# Apply-Migration (Obsolete)
#

function Apply-Migration {
    # TODO: Remove before RTM
    throw 'Apply-Migration has been removed. Use Update-Database instead.'
}

#
# Script-Migration
#

Register-TabExpansion Script-Migration @{
    From = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project }
    To = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project }
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Generates a SQL script from migrations.

.DESCRIPTION
    Generates a SQL script from migrations.

.PARAMETER From
    Specifies the starting migration. If omitted, '0' (the initial database) is used.

.PARAMETER To
    Specifies the ending migration. If omitted, the last migration is used.

.PARAMETER Idempotent
    Generates an idempotent script that can used on a database at any migration.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the start-up project to use. If omitted, the solution's start-up project is used.

.LINK
    Update-Database
    about_EntityFramework
#>
function Script-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(ParameterSetName = 'WithoutTo')]
        [Parameter(ParameterSetName = 'WithTo', Mandatory = $true)]
        [string] $From,
        [Parameter(ParameterSetName = 'WithTo', Mandatory = $true)]
        [string] $To,
        [switch] $Idempotent,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject)

    $values = ProcessCommonParameters $Context $Project $StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName
    $dteStartupProject = $values.StartupProject

    $script = InvokeOperation $dteProject ScriptMigration @{
        fromMigrationName = $From
        toMigrationName = $To
        idempotent = [bool]$Idempotent
        contextTypeName = $contextTypeName
    } -startupProject $dteStartupProject

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
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Removes the last migration.

.DESCRIPTION
    Removes the last migration.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the start-up project to use. If omitted, the solution's start-up project is used.

.LINK
    Add-Migration
    about_EntityFramework
#>
function Remove-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param ([string] $Context, [string] $Project, [string] $StartupProject)

    $values = ProcessCommonParameters $Context $Project $StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName
    $dteStartupProject = $values.StartupProject

    $filesToDelete = InvokeOperation $dteProject RemoveMigration @{
        contextTypeName = $contextTypeName
    } -startupProject $dteStartupProject

    $filesToDelete | ?{ Test-Path $_ } | %{ (GetProjectItem $dteProject $_).Delete() }
}

#
# Scaffold-DbContext
#

Register-TabExpansion Scaffold-DbContext @{
    Provider = { param ($tabExpansionContext) GetProviders $tabExpansionContext.Project }
    Project = { GetProjects }
}

<#
.SYNOPSIS
    Scaffolds a DbContext and entity type classes for a specified database.

.DESCRIPTION
    Scaffolds a DbContext and entity type classes for a specified database.

.PARAMETER Connection
    Specifies the connection string of the database.

.PARAMETER Provider
    Specifies the provider to use. For example, EntityFramework.SqlServer.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.LINK
    Scaffold-DbContextTemplate
    about_EntityFramework
#>
function Scaffold-DbContext {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Connection,
        [Parameter(Position = 1, Mandatory = $true)]
        [string] $Provider,
        [string] $Project)

    $values = ProcessCommonParameters -projectName $Project
    $dteProject = $values.Project

    $artifacts = InvokeOperation $dteProject ReverseEngineer @{
        connectionString = $Connection
        provider = $Provider
    }

    $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole
}

#
# Scaffold-DbContextTemplate
#

Register-TabExpansion Scaffold-DbContextTemplate @{
    Provider = { param ($tabExpansionContext) GetProviders $tabExpansionContext.Project }
    Project = { GetProjects }
}

<#
.SYNOPSIS
    Scaffolds customizable DbContext and entity type templates to use during Scaffold-DbContext.

.DESCRIPTION
    Scaffolds customizable DbContext and entity type templates to use during Scaffold-DbContext.

.PARAMETER Provider
    Specifies the provider to use. For example, EntityFramework.SqlServer.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.LINK
    Scaffold-DbContext
    about_EntityFramework
#>
function Scaffold-DbContextTemplate {
    [CmdletBinding(PositionalBinding = $false)]
    param ([Parameter(Position = 0, Mandatory = $true)] [string] $Provider, [string] $Project)

    $values = ProcessCommonParameters -projectName $Project
    $dteProject = $values.Project

    $artifacts = InvokeOperation $dteProject CustomizeReverseEngineer @{
        provider = $Provider
    }

    $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole
}

#
# Enable-Migrations (Obsolete)
#

function Enable-Migrations {
    # TODO: Link to some docs on the changes to Migrations
    Write-Warning 'Enable-Migrations is obsolete. Use Add-Migration to start using Migrations.'
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

function ProcessCommonParameters($contextTypeName, $projectName, $startupProjectName) {
    $project = GetProject $projectName

    if (!$contextTypeName -and $project.ProjectName -eq $EFDefaultParameterValues.ProjectName) {
        $contextTypeName = $EFDefaultParameterValues.ContextTypeName
    }

    $startupProject = GetStartupProject $startupProjectName $project

    return @{
        Project = $project
        ContextTypeName = $contextTypeName
        StartupProject = $startupProject
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

function InvokeOperation($project, $operation, $arguments = @{}, $startupProject = $project, [switch] $skipBuild) {

    $package = Get-Package -ProjectName $project.ProjectName | ? Id -eq EntityFramework.Commands
    if (!($package)) {
        throw 'Cannot execute this operation because EntityFramework.Commands is not installed in the targeted project'
    }

    $projectName = $project.ProjectName

    Write-Verbose "Using project '$projectName'"

    if (!$skipBuild) {
        Write-Verbose 'Build started...'

        # TODO: Only build required project. Don't use BuildProject, you can't specify platform
        $solutionBuild = $DTE.Solution.SolutionBuild
        $solutionBuild.Build($true)
        if ($solutionBuild.LastBuildInfo) {
            throw "Build failed."
        }

        Write-Verbose 'Build succeeded.'
    }

    $startupProjectName = $startupProject.ProjectName

    Write-Verbose "Using start-up project '$startupProjectName'."

    if (![Type]::GetType('Microsoft.Data.Entity.Commands.ILogHandler')) {
        Add-Type @(
            Join-Path $PSScriptRoot IHandlers.cs
            Join-Path $PSScriptRoot Handlers.cs
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

    $startupOutputPath = GetProperty $startupProject.ConfigurationManager.ActiveConfiguration.Properties OutputPath
    $startupProperties = $startupProject.Properties
    $startupFullPath = GetProperty $startupProperties FullPath
    $startupTargetDir = Join-Path $startupFullPath $startupOutputPath

    $webConfig = GetProjectItem $startupProject 'Web.Config'
    $appConfig = GetProjectItem $startupProject 'App.Config'

    Write-Verbose "Using application base '$targetDir'."

    if ($webConfig) {
        $configurationFile = GetProperty $webConfig.Properties FullPath
        $dataDirectory = Join-Path $startupFullPath 'App_Data'
        Write-Verbose "Using application configuration '$configurationFile'"
    }
    elseif ($appConfig) {
        $configurationFile = GetProperty $appConfig.Properties FullPath
        $dataDirectory = $startupTargetDir
        Write-Verbose "Using application configuration '$configurationFile'"
    }
    else {
        Write-Verbose 'No configuration file found.'
        $dataDirectory = $startupTargetDir
    }

    Write-Verbose "Using data directory '$dataDirectory'"

    $info = New-Object AppDomainSetup -Property @{
        ApplicationBase = $targetDir
        ShadowCopyFiles = 'true'
        ConfigurationFile = $configurationFile
    }

    $domain = [AppDomain]::CreateDomain('EntityFrameworkDesignDomain', $null, $info)
    $domain.SetData('DataDirectory', $dataDirectory)
    try {
        $assemblyName = 'EntityFramework.Commands'
        $typeName = 'Microsoft.Data.Entity.Commands.Executor'
        $targetFileName = GetProperty $properties OutputFileName
        $targetPath = Join-Path $targetDir $targetFileName
        $startupTargetFileName = GetProperty $startupProperties OutputFileName
        $startupTargetPath = Join-Path $startupTargetDir $startupTargetFileName
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
                    startupTargetPath = [string]$startupTargetPath
                    projectDir = $fullPath
                    rootNamespace = $rootNamespace
                }
            ),
            $null,
            $null)

        $resultHandler = New-Object Microsoft.Data.Entity.Commands.ResultHandler
        $currentDirectory = [IO.Directory]::GetCurrentDirectory()

        Write-Verbose "Using current directory '$startupTargetDir'."

        [IO.Directory]::SetCurrentDirectory($startupTargetDir)
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

    if (Split-Path $path -IsAbsolute) {
        $path = $path.Substring($fullPath.Length)
    }

    $itemDirectory = (Split-Path $path -Parent)

    $projectItems = $project.ProjectItems
    if ($itemDirectory) {
        $directories = $itemDirectory.Split('\')
        $directories | %{
            $projectItems = $projectItems.Item($_).ProjectItems
        }
    }

    $itemName = Split-Path $path -Leaf

    try {
        return $projectItems.Item($itemName)
    }
    catch [Exception] {
    }

    return $null
}

function GetStartUpProject($name, $fallbackProject) {
    if ($name) {
        return Get-Project $name
    }

    $startupProjectPaths = $DTE.Solution.SolutionBuild.StartupProjects
    if ($startupProjectPaths) {
        if ($startupProjectPaths.Length -eq 1) {
            $startupProjectPath = $startupProjectPaths[0]
            if (!(Split-Path -IsAbsolute $startupProjectPath)) {
                $solutionPath = Split-Path (GetProperty $DTE.Solution.Properties Path)
                $startupProjectPath = Join-Path $solutionPath $startupProjectPath -Resolve
            }

            $startupProject = GetSolutionProjects | ?{
                try {
                    $fullName = $_.FullName
                }
                catch [NotImplementedException] {
                    return $false
                }

                if ($fullName -and $fullName.EndsWith('\')) {
                    $fullName = $fullName.Substring(0, $fullName.Length - 1)
                }

                return $fullName -eq $startupProjectPath
            }
            if ($startupProject) {
                return $startupProject
            }

            Write-Warning "Unable to resolve start-up project '$startupProjectPath'."
        }
        else {
            Write-Verbose 'More than one start-up project found.'
        }
    }
    else {
        Write-Verbose 'No start-up project found.'
    }

    return $fallbackProject
}

function GetSolutionProjects() {
    $projects = New-Object System.Collections.Stack

    $DTE.Solution.Projects | %{
        $projects.Push($_)
    }

    while ($projects.Count -ne 0) {
        $project = $projects.Pop();

        # NOTE: This line is similar to doing a "yield return" in C#
        $project

        if ($project.ProjectItems) {
            $project.ProjectItems | ?{ $_.SubProject } | %{
                $projects.Push($_.SubProject)
            }
        }
    }
}

function GetProviders($projectName) {
    if (!($projectName)) {
        $projectName = (Get-Project).ProjectName
    }

    return Get-Package -ProjectName $projectName | select -ExpandProperty Id
}

