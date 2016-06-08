$ErrorActionPreference = 'Stop'

$EFDefaultParameterValues = @{
    ProjectName = ''
    ContextTypeName = ''
}

#
# Use-DbContext
#

Register-TabExpansion Use-DbContext @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
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

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    about_EntityFrameworkCore
#>
function Use-DbContext {
    [CmdletBinding(PositionalBinding = $false)]
    param ([Parameter(Position = 0, Mandatory = $true)] [string] $Context, [string] $Project, [string] $StartupProject, [string] $Environment)

    $dteProject = GetProject $Project
    $dteStartupProject = GetStartupProject $StartupProject $dteProject
    if (IsDotNetProject $dteProject) {
        $contextTypes = GetContextTypes $Project $StartupProject $Environment
        $candidates = $contextTypes | ? { $_ -ilike "*$Context" }
        $exactMatch = $contextTypes | ? { $_ -eq $Context }
        if ($candidates.length -gt 1 -and $exactMatch -is "String") {
            $candidates = $exactMatch
        }

        if ($candidates.length -lt 1) {
            throw "No DbContext named '$Context' was found"
        } elseif ($candidates.length -gt 1 -and !($candidates -is "String")) {
            throw "More than one DbContext named '$Context' was found. Specify which one to use by providing its fully qualified name."
        }

        $contextTypeName=$candidates
    } else {
        $contextTypeName = InvokeOperation $dteStartupProject $Environment $dteProject GetContextType @{ name = $Context }
    }

    $EFDefaultParameterValues.ContextTypeName = $contextTypeName
    $EFDefaultParameterValues.ProjectName = $dteProject.ProjectName
}

#
# Add-Migration
#

Register-TabExpansion Add-Migration @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
    # disables tab completion on output dir
    OutputDir = { }
}

<#
.SYNOPSIS
    Adds a new migration.

.DESCRIPTION
    Adds a new migration.

.PARAMETER Name
    Specifies the name of the migration.

.PARAMETER OutputDir
    The directory (and sub-namespace) to use. If omitted, "Migrations" is used. Relative paths are relative to project directory.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Remove-Migration
    Update-Database
    about_EntityFrameworkCore
#>
function Add-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Name,
        [string] $OutputDir,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    Hint-Upgrade $MyInvocation.MyCommand
    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    if (IsDotNetProject $dteProject) {
        $options = ProcessCommonDotnetParameters $Environment $contextTypeName
        if($OutputDir) {
            $options += "--output-dir", (NormalizePath $OutputDir)
        }
        $files = InvokeDotNetEf $dteProject $dteStartupProject -json migrations add $Name @options
        $DTE.ItemOperations.OpenFile($files.MigrationFile) | Out-Null
    }
    else {
        $artifacts = InvokeOperation $dteStartupProject $Environment $dteProject AddMigration @{
            name = $Name
            outputDir = $OutputDir
            contextType = $contextTypeName
        }
        
        $dteProject.ProjectItems.AddFromFile($artifacts.MigrationFile) | Out-Null

        try {
            $dteProject.ProjectItems.AddFromFile($artifacts.MetadataFile) | Out-Null
        } catch {
            # in some SKUs the call to add MigrationFile will automatically add the MetadataFile because it is named ".Designer.cs"
            # this will throw a non fatal error when -OutputDir is outside the main project directory
        }

        $dteProject.ProjectItems.AddFromFile($artifacts.SnapshotFile) | Out-Null
        $DTE.ItemOperations.OpenFile($artifacts.MigrationFile) | Out-Null
        ShowConsole
    }

    Write-Output 'To undo this action, use Remove-Migration.'
}

#
# Update-Database
#

Register-TabExpansion Update-Database @{
    Migration = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
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
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Script-Migration
    about_EntityFrameworkCore
#>
function Update-Database {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0)]
        [string] $Migration,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    Hint-Upgrade $MyInvocation.MyCommand
    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    if (IsDotNetProject $dteProject) {
        $options = ProcessCommonDotnetParameters $Environment $contextTypeName
        InvokeDotNetEf $dteProject $dteStartupProject database update $Migration @options | Out-Null
        Write-Output "Done."
    } else {
        if (IsUwpProject $dteProject) {
            throw 'Update-Database should not be used with Universal Windows apps. Instead, call DbContext.Database.Migrate() at runtime.'
        }

        InvokeOperation $dteStartupProject $Environment $dteProject UpdateDatabase @{
            targetMigration = $Migration
            contextType = $contextTypeName
        }
    }
}

#
# Script-Migration
#

Register-TabExpansion Script-Migration @{
    From = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    To = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
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
    Generates an idempotent script that can be used on a database at any migration.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Update-Database
    about_EntityFrameworkCore
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
        [string] $StartupProject,
        [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $fullPath = GetProperty $dteProject.Properties FullPath
    $intermediatePath = if (IsDotNetProject $dteProject) { "obj\Debug\" }
        else { GetProperty $dteProject.ConfigurationManager.ActiveConfiguration.Properties IntermediatePath }
    $fullIntermediatePath = Join-Path $fullPath $intermediatePath
    $fileName = [IO.Path]::GetRandomFileName()
    $fileName = [IO.Path]::ChangeExtension($fileName, '.sql')
    $scriptFile = Join-Path $fullIntermediatePath $fileName

    if (IsDotNetProject $dteProject) {
        $options = ProcessCommonDotnetParameters $Environment $contextTypeName

        $options += "--output",$scriptFile
        if ($Idempotent) {
            $options += ,"--idempotent"
        }

        InvokeDotNetEf $dteProject $dteStartupProject migrations script $From $To @options | Out-Null

        $DTE.ItemOperations.OpenFile($scriptFile) | Out-Null

    } else {
        $script = InvokeOperation $dteStartupProject $Environment $dteProject ScriptMigration @{
            fromMigration = $From
            toMigration = $To
            idempotent = [bool]$Idempotent
            contextType = $contextTypeName
        }
        try {
            # NOTE: Certain SKUs cannot create new SQL files, including xproj
            $window = $DTE.ItemOperations.NewFile('General\Sql File')
            $textDocument = $window.Document.Object('TextDocument')
            $editPoint = $textDocument.StartPoint.CreateEditPoint()
            $editPoint.Insert($script)
        }
        catch {
            $script | Out-File $scriptFile -Encoding utf8
            $DTE.ItemOperations.OpenFile($scriptFile) | Out-Null
        }
    }

    ShowConsole
}

#
# Remove-Migration
#

Register-TabExpansion Remove-Migration @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
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
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.PARAMETER Force
    Removes the last migration without checking the database. If the last migration has been applied to the database, you will need to manually reverse the changes it made.

.LINK
    Add-Migration
    about_EntityFrameworkCore
#>
function Remove-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param ([string] $Context, [string] $Project, [string] $StartupProject, [string] $Environment, [switch] $Force)

    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName
    $dteStartupProject = $values.StartupProject
    $forceRemove = $Force -or (IsUwpProject $dteProject)

    if (IsDotNetProject $dteProject) {
        $options = ProcessCommonDotnetParameters $Environment $contextTypeName
        if ($forceRemove) {
            $options += ,"--force"
        }
        InvokeDotNetEf $dteProject $dteStartupProject migrations remove @options | Out-Null
        Write-Output "Done."
    } else {
        $filesToRemove = InvokeOperation $dteStartupProject $Environment $dteProject RemoveMigration @{
            contextType = $contextTypeName
            force = [bool]$forceRemove
        }

        $filesToRemove | %{
            $projectItem = GetProjectItem $dteProject $_
            if ($projectItem) {
                $projectItem.Remove()
            }
        }
    }
}

#
# Scaffold-DbContext
#

Register-TabExpansion Scaffold-DbContext @{
    Provider = { param ($tabExpansionContext) GetProviders $tabExpansionContext.Project }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Scaffolds a DbContext and entity type classes for a specified database.

.DESCRIPTION
    Scaffolds a DbContext and entity type classes for a specified database.

.PARAMETER Connection
    Specifies the connection string of the database.

.PARAMETER Provider
    Specifies the provider to use. For example, Microsoft.EntityFrameworkCore.SqlServer.

.PARAMETER OutputDir
    Specifies the directory to use to output the classes. If omitted, the top-level project directory is used.

.PARAMETER Context
    Specifies the name of the generated DbContext class.

.PARAMETER Schemas
    Specifies the schemas for which to generate classes.

.PARAMETER Tables
    Specifies the tables for which to generate classes.

.PARAMETER DataAnnotations
    Use DataAnnotation attributes to configure the model where possible. If omitted, the output code will use only the fluent API.

.PARAMETER Force
    Force scaffolding to overwrite existing files. Otherwise, the code will only proceed if no output files would be overwritten.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    about_EntityFrameworkCore
#>
function Scaffold-DbContext {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Connection,
        [Parameter(Position = 1, Mandatory =  $true)]
        [string] $Provider,
        [string] $OutputDir,
        [string] $Context,
        [string[]] $Schemas = @(),
        [string[]] $Tables = @(),
        [switch] $DataAnnotations,
        [switch] $Force,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project

    if (IsDotNetProject $dteProject) {
        $options = ProcessCommonDotnetParameters $Environment $Context
        if ($OutputDir) {
            $options += "--output-dir",(NormalizePath $OutputDir)
        }
        if ($DataAnnotations) {
            $options += ,"--data-annotations"
        }
        if ($Force) {
            $options += ,"--force"
        }
        $options += $Schemas | % { "--schema", $_ }
        $options += $Tables | % { "--table", $_ }

        InvokeDotNetEf $dteProject $dteStartupProject dbcontext scaffold $Connection $Provider @options | Out-Null
    } else {
        $artifacts = InvokeOperation $dteStartupProject $Environment $dteProject ReverseEngineer @{
            connectionString = $Connection
            provider = $Provider
            outputDir = $OutputDir
            dbContextClassName = $Context
            schemaFilters = $Schemas
            tableFilters = $Tables
            useDataAnnotations = [bool]$DataAnnotations
            overwriteFiles = [bool]$Force
        }

        $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
        $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null

        ShowConsole
    }
}

#
# Enable-Migrations (Obsolete)
#

function Enable-Migrations {
    # TODO: Link to some docs on the changes to Migrations
    Hint-Upgrade $MyInvocation.MyCommand
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

function GetContextTypes($projectName, $startupProjectName, $environment) {
    $values = ProcessCommonParameters $startupProjectName $projectName
    $startupProject = $values.StartupProject
    $project = $values.Project

    if (IsDotNetProject $startupProject) {
        $options = ProcessCommonDotnetParameters $environment
        $types = InvokeDotNetEf $startupProject $startupProject -json -skipBuild dbcontext list @options
        return $types | %{ $_.fullName }
    } else {
        $contextTypes = InvokeOperation $startupProject $environment $project GetContextTypes -skipBuild
        return $contextTypes | %{ $_.SafeName }
    }
}

function GetMigrations($contextTypeName, $projectName, $startupProjectName, $environment) {
    $values = ProcessCommonParameters $startupProjectName $projectName $contextTypeName
    $startupProject = $values.StartupProject
    $project = $values.Project
    $contextTypeName = $values.ContextTypeName

    if (IsDotNetProject $startupProject) {
        $options = ProcessCommonDotnetParameters $environment $contextTypeName
        $migrations = InvokeDotNetEf $startupProject $startupProject -json -skipBuild migrations list @options
        return $migrations | %{ $_.safeName }
    }
    else {
        $migrations = InvokeOperation $startupProject $environment $project GetMigrations @{ contextTypeName = $contextTypeName } -skipBuild
        return $migrations | %{ $_.SafeName }
    }
}

function ProcessCommonParameters($startupProjectName, $projectName, $contextTypeName) {
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

function NormalizePath($path) {
    try {
        $pathInfo = Resolve-Path -LiteralPath $path
        return $pathInfo.Path.TrimEnd([IO.Path]::DirectorySeparatorChar)
    } 
    catch {
        # when directories don't exist yet
        return $path.TrimEnd([IO.Path]::DirectorySeparatorChar)
    }
}

function ProcessCommonDotnetParameters($environment, $contextTypeName) {
    $options=@()
    if ($environment) {
        $options += "--environment",$environment
    }
    if ($contextTypeName) {
        $options += "--context",$contextTypeName
    }
    return $options
}

function IsDotNetProject($project) {
    $project.FileName -like "*.xproj" -or $project.Kind -eq "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}"
}

function IsUwpProject($project) {
    $targetFrameworkMoniker = GetProperty $project.Properties TargetFrameworkMoniker
    $frameworkName = New-Object System.Runtime.Versioning.FrameworkName $targetFrameworkMoniker
    return $frameworkName.Identifier -eq '.NETCore'
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

function InvokeDotNetEf($dteProject, $dteStartupProject, [switch] $json, [switch] $skipBuild) {

    if (!(IsDotNetProject $dteProject) -or !(IsDotNetProject $dteStartupProject)) {
        Write-Warning "This command may fail unless both the targeted project and startup project are ASP.NET Core or .NET Core projects."
    }

    if ($env:DOTNET_INSTALL_DIR) {
        $dotnet = Join-Path $env:DOTNET_INSTALL_DIR dotnet.exe
    } else {
        $cmd = Get-Command dotnet -ErrorAction Ignore # searches $env:PATH
        if ($cmd) {
            $dotnet = $cmd.Path
        }
    }

    if (!(Test-Path $dotnet)) {
        throw "Could not find .NET Core CLI (dotnet.exe) in the PATH or DOTNET_INSTALL_DIR environment variables. .NET Core CLI is required to execute EF commands on this project type."
    }

    Write-Debug "Using $dotnet"
    $targetFullPath = GetProperty $dteProject.Properties FullPath
    $targetProjectJson = Join-Path $targetFullPath project.json
    try {
        Write-Debug "Reading $targetProjectJson"
        $projectDef = Get-Content $targetProjectJson -Raw | ConvertFrom-Json
    } catch {
        Write-Verbose $_.Exception.Message
        throw "Invalid JSON file in $targetProjectJson"
    }
    if ($projectDef.tools) {
        $t=$projectDef.tools | Get-Member Microsoft.EntityFrameworkCore.Tools
    }
    if (!$t) {
        $projectName = $dteProject.ProjectName
        throw "Cannot execute this command because 'Microsoft.EntityFrameworkCore.Tools' is not installed in project '$projectName'. Add 'Microsoft.EntityFrameworkCore.Tools' to the 'tools' section in project.json. See http://go.microsoft.com/fwlink/?LinkId=798221 for more details."
    }

    $arguments=@()

    $startupProjectPath =  GetProperty $dteStartupProject.Properties FullPath
    $arguments += "--startup-project", (NormalizePath $startupProjectPath)

    $startupProjectName =  $dteStartupProject.ProjectName
    Write-Verbose "Using startup project '$startupProjectName'"

    $config = $dteStartupProject.ConfigurationManager.ActiveConfiguration.ConfigurationName
    $arguments += "--configuration", $config
    Write-Debug "Using configuration $config"

    $buildBasePath = GetProperty $dteStartupProject.ConfigurationManager.ActiveConfiguration.Properties OutputPath
    $arguments += "--build-base-path", (NormalizePath $buildBasePath)
    Write-Debug "Using build base path $buildBasePath"
    
    if ($skipBuild) {
        $arguments += ,"--no-build"
    }

    $arguments += $args

    if ($json) {
        $arguments += ,"--json"
    }

    if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent) {
        $arguments += ,"--verbose"
    }

    $arguments = $arguments | ? { $_ } | % { "'$($_ -replace "'", "''" )'" }

    $output = $null

    $command = "ef $($arguments -join ' ')"
    try {
        Write-Verbose "Working directory: $targetFullPath"
        Push-Location $targetFullPath
        $ErrorActionPreference='SilentlyContinue'
        Write-Verbose "Executing command: dotnet $command"
        # TODO don't use invoke-expression.
        # This will require running dotnet-build as a separate command because build
        # warnings still appear in stderr
        $stdout = Invoke-Expression "& '$dotnet' $command" -ErrorVariable stderr
        $exit = $LASTEXITCODE
        $stdout | Out-String | Write-Verbose
        Write-Debug "Finish executing command with code $exit"
        if ($exit -ne 0) {
            if (!($stderr)) {
                if (!($stdout)) {
                    # This should never happen
                    throw "An unexpected error occurred."
                }
                # most often occurs when Microsoft.EntityFrameworkCore.Tools didn't install
                throw $stdout
            }
            throw $stderr
        }

        if ($json) {
            Write-Debug "Parsing json output"
            $startLine = $stdout.IndexOf("//BEGIN") + 1
            $endLine = $stdout.IndexOf("//END") - 1
            $output = $stdout[$startLine..$endLine] -join [Environment]::NewLine | ConvertFrom-Json
        } else {
            $output = $stdout -join [Environment]::NewLine
        }
    }
    finally {
        $ErrorActionPreference='Stop'
        Pop-Location
    }
    return $output
}

function InvokeOperation($startupProject, $environment, $project, $operation, $arguments = @{}, [switch] $skipBuild) {
    $startupProjectName = $startupProject.ProjectName

    Write-Verbose "Using startup project '$startupProjectName'."

    $projectName = $project.ProjectName

    Write-Verbose "Using project '$projectName'"

    if (IsDotNetProject $startupProject) {
        throw "This command cannot use '$startupProjectName' as the startup project because '$projectName' is not an ASP.NET Core or .NET Core project"
    }

    $package = Get-Package -ProjectName $startupProjectName | ? Id -eq Microsoft.EntityFrameworkCore.Tools
    if (!($package)) {
        throw "Cannot execute this command because Microsoft.EntityFrameworkCore.Tools is not installed in the startup project '$startupProjectName'."
    }

    if (!$skipBuild) {

        if (IsUwpProject $startupProject) {
            $config = $startupProject.ConfigurationManager.ActiveConfiguration.ConfigurationName
            $configProperties = $startupProject.ConfigurationManager.ActiveConfiguration.Properties
            $isNative = (GetProperty $configProperties ProjectN.UseDotNetNativeToolchain) -eq 'True'

            if ($isNative) {
                throw "Cannot run in '$config' mode because 'Compile with the .NET Native tool chain' is enabled. Disable this setting or use a different configuration and try again."
            }
        }

        Write-Verbose 'Build started...'

        # TODO: Only build required project. Don't use BuildProject, you can't specify platform
        $solutionBuild = $DTE.Solution.SolutionBuild
        $solutionBuild.Build($true)
        if ($solutionBuild.LastBuildInfo) {
            throw "Build failed."
        }

        Write-Verbose 'Build succeeded.'
    }

    if (![Type]::GetType('Microsoft.EntityFrameworkCore.Design.OperationResultHandler')) {
        Add-Type -Path (Join-Path $PSScriptRoot OperationHandlers.cs) -CompilerParameters (
            New-Object CodeDom.Compiler.CompilerParameters -Property @{
                CompilerOptions = '/d:NET451'
            })
    }

    $logHandler = New-Object Microsoft.EntityFrameworkCore.Design.OperationLogHandler @(
        { param ($message) Write-Error $message }
        { param ($message) Write-Warning $message }
        { param ($message) Write-Host $message }
        { param ($message) Write-Verbose $message }
        { param ($message) Write-Debug $message }
    )

    $properties = $project.Properties
    $fullPath = GetProperty $properties FullPath

    $startupOutputPath = GetProperty $startupProject.ConfigurationManager.ActiveConfiguration.Properties OutputPath
    $startupProperties = $startupProject.Properties
    $startupFullPath = GetProperty $startupProperties FullPath
    $startupTargetDir = Join-Path $startupFullPath $startupOutputPath

    $webConfig = GetProjectItem $startupProject 'Web.Config'
    $appConfig = GetProjectItem $startupProject 'App.Config'

    if ($webConfig) {
        $configurationFile = GetProperty $webConfig.Properties FullPath
        $dataDirectory = Join-Path $startupFullPath 'App_Data'
    }
    elseif ($appConfig) {
        $configurationFile = GetProperty $appConfig.Properties FullPath
    }

    Write-Verbose "Using application base '$startupTargetDir'."

    $info = New-Object AppDomainSetup -Property @{
        ApplicationBase = $startupTargetDir
        ShadowCopyFiles = 'true'
    }

    if ($configurationFile) {
        Write-Verbose "Using application configuration '$configurationFile'"
        $info.ConfigurationFile = $configurationFile
    }
    else {
        Write-Verbose 'No configuration file found.'
    }

    $domain = [AppDomain]::CreateDomain('EntityFrameworkCoreDesignDomain', $null, $info)
    if ($dataDirectory) {
        Write-Verbose "Using data directory '$dataDirectory'"
        $domain.SetData('DataDirectory', $dataDirectory)
    }
    try {
        $commandsAssembly = 'Microsoft.EntityFrameworkCore.Tools'
        $commandsAssemblyFile = Join-Path $startupTargetDir "$commandsAssembly.dll"
        if (!(Test-Path $commandsAssemblyFile)) {
            Copy-Item "$PSScriptRoot/../lib/net451/$commandsAssembly.dll" $startupTargetDir
            $removeCommandsAssembly = $True
        }
        $operationExecutorTypeName = 'Microsoft.EntityFrameworkCore.Design.OperationExecutor'
        $targetAssemblyName = GetProperty $properties AssemblyName
        $startupAssemblyName = GetProperty $startupProperties AssemblyName
        $rootNamespace = GetProperty $properties RootNamespace

        $executor = $domain.CreateInstanceAndUnwrap(
            $commandsAssembly,
            $operationExecutorTypeName,
            $false,
            0,
            $null,
            @(
                [MarshalByRefObject]$logHandler,
                @{
                    startupTargetName = $startupAssemblyName
                    targetName = $targetAssemblyName
                    environment = $environment
                    projectDir = $fullPath
                    startupProjectDir = $startupFullPath
                    rootNamespace = $rootNamespace
                }
            ),
            $null,
            $null)

        $resultHandler = New-Object Microsoft.EntityFrameworkCore.Design.OperationResultHandler
        $currentDirectory = [IO.Directory]::GetCurrentDirectory()

        Write-Verbose "Using current directory '$startupTargetDir'."

        [IO.Directory]::SetCurrentDirectory($startupTargetDir)
        try {
            $domain.CreateInstance(
                $commandsAssembly,
                "$operationExecutorTypeName+$operation",
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
        if ($removeCommandsAssembly) {
            Remove-Item $commandsAssemblyFile
        }
    }

    if ($resultHandler.ErrorType) {
        if ($resultHandler.ErrorType -eq 'Microsoft.EntityFrameworkCore.Design.OperationException') {
            Write-Verbose $resultHandler.ErrorStackTrace
        }
        else {
            Write-Host $resultHandler.ErrorStackTrace
        }

        throw $resultHandler.ErrorMessage
    }
    if ($resultHandler.HasResult) {
        return $resultHandler.Result
    }
}

function GetProperty($properties, $propertyName) {
    try {
        return $properties.Item($propertyName).Value
    } catch {
        return $null
    }
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

            Write-Warning "Unable to resolve startup project '$startupProjectPath'."
        }
        else {
            Write-Verbose 'More than one startup project found.'
        }
    }
    else {
        Write-Verbose 'No startup project found.'
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

function Hint-Upgrade ($name) {
    if (Get-Module | ? Name -eq EntityFramework) {
        Write-Warning "Both Entity Framework Core and Entity Framework 6.x commands are installed. The Entity Framework Core version is executing. You can fully qualify the command to select which one to execute, 'EntityFramework\$name' for EF6.x and 'EntityFrameworkCore\$name' for EF Core."
    }
}