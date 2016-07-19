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

    $contextTypes = GetContextTypes $Project $StartupProject $Environment
    $candidates = $contextTypes | ? { $_ -ilike "*$Context" }
    $exactMatch = $contextTypes | ? { $_ -eq $Context }
    if ($candidates.length -gt 1 -and $exactMatch -is 'String') {
        $candidates = $exactMatch
    }

    if ($candidates.length -lt 1) {
        throw "No DbContext named '$Context' was found"
    } elseif ($candidates.length -gt 1 -and !($candidates -is 'String')) {
        throw "More than one DbContext named '$Context' was found. Specify which one to use by providing its fully qualified name."
    }

    $EFDefaultParameterValues.ContextTypeName = $candidates
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
    $values = ProcessCommonParameters $StartupProject $Project $Context $Environment

    $options = @()
    if($OutputDir) {
        $options += '--output-dir', $OutputDir
    }
    $artifacts = InvokeOperation $values -json migrations add $Name @options

    if (!(IsDotNetProject $values.Project)) {
        if ($artifacts.MigrationFile) {
            $values.Project.ProjectItems.AddFromFile($artifacts.MigrationFile) | Out-Null
        }
        try {
            $values.Project.ProjectItems.AddFromFile($artifacts.MetadataFile) | Out-Null
        } catch {
            # in some SKUs the call to add MigrationFile will automatically add the MetadataFile because it is named ".Designer.cs"
            # this will throw a non fatal error when -OutputDir is outside the main project directory
        }

        if ($artifacts.SnapshotFile) {
            $values.Project.ProjectItems.AddFromFile($artifacts.SnapshotFile) | Out-Null
        }
    }

    if ($artifacts.MigrationFile) {
        $DTE.ItemOperations.OpenFile($artifacts.MigrationFile) | Out-Null
    }
    ShowConsole
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
    $values = ProcessCommonParameters $StartupProject $Project $Context $Environment
    if (IsUwpProject $value.Project) {
        throw 'Update-Database should not be used with Universal Windows apps. Instead, call DbContext.Database.Migrate() at runtime.'
    }

    InvokeOperation $values database update $Migration | Out-Null
    Write-Output 'Done.'
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

    $values = ProcessCommonParameters $StartupProject $Project $Context $Environment

    $fullPath = GetProperty $values.Project.Properties FullPath
    $intermediatePath = if (IsDotNetProject $values.Project) { 'obj\Debug\' }
        else { GetProperty $values.Project.ConfigurationManager.ActiveConfiguration.Properties IntermediatePath }
    $fullIntermediatePath = Join-Path $fullPath $intermediatePath
    $fileName = [IO.Path]::GetRandomFileName()
    $fileName = [IO.Path]::ChangeExtension($fileName, '.sql')
    $scriptFile = Join-Path $fullIntermediatePath $fileName

    $options = '--output',$scriptFile
    if ($Idempotent) {
        $options += ,'--idempotent'
    }

    InvokeOperation $values migrations script $From $To @options | Out-Null

    $DTE.ItemOperations.OpenFile($scriptFile) | Out-Null

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

    $values = ProcessCommonParameters $StartupProject $Project $Context $Environment

    $forceRemove = $Force -or (IsUwpProject $values.Project)

    $options=@()
    if ($forceRemove) {
        $options += ,'--force'
    }

    $result = InvokeOperation $values -json migrations remove @options

    if (!(IsDotNetProject $values.Project) -and $result.files) {
        $result.files | %{
            $projectItem = GetProjectItem $values.Project $_
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

    $values = ProcessCommonParameters $StartupProject $Project $Context $Environment

    $options = @()
    if ($OutputDir) {
        $options += '--output-dir', $OutputDir
    }
    if ($DataAnnotations) {
        $options += ,'--data-annotations'
    }
    if ($Force) {
        $options += ,'--force'
    }
    $options += $Schemas | % { '--schema', $_ }
    $options += $Tables | % { '--table', $_ }

    $result = InvokeOperation $values -json dbcontext scaffold $Connection $Provider @options
   
    if (!(IsDotNetProject $values.Project) -and $result.files) {
        $result.files | %{ $values.Project.ProjectItems.AddFromFile($_) | Out-Null }
        $DTE.ItemOperations.OpenFile($result.files[0]) | Out-Null
    }

    ShowConsole
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
    $values = ProcessCommonParameters $startupProjectName $projectName $null $environment -suppressContextOption
    $types = InvokeOperation $values -json -skipBuild dbcontext list
    return $types | %{ $_.safeName }
}

function GetMigrations($contextTypeName, $projectName, $startupProjectName, $environment) {
    $values = ProcessCommonParameters $startupProjectName $projectName $contextTypeName $environment
    $migrations = InvokeOperation $values -json -skipBuild migrations list 
    return $migrations | %{ $_.safeName }
}

function IsDotNetProject($project) {
    $project.FileName -like '*.xproj' -or $project.Kind -eq '{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}'
}

function IsUwpProject($project) {
    $targetDesription = GetProperty $project.Properties Project.TargetDescriptions
    return $targetDesription -eq 'Universal Windows'
}

function IsClassLibrary($project) {
    if (IsDotNetProject $project) {
        return GetProperty $project.Properties IsClasslibraryProject
    }
    $type = GetProperty $project.Properties OutputType
    return $type -eq 2 -or $type -eq 'Library'
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

function GetDotNet {
    if ($env:DOTNET_INSTALL_DIR) {
        $dotnet = Join-Path $env:DOTNET_INSTALL_DIR dotnet.exe
    } else {
        $cmd = Get-Command dotnet -ErrorAction Ignore # searches $env:PATH
        if ($cmd) {
            $dotnet = $cmd.Path
        }
    }

    if (!(Test-Path $dotnet)) {
        throw 'Could not find .NET Core CLI (dotnet.exe) in the PATH or DOTNET_INSTALL_DIR environment variables. .NET Core CLI is required to execute EF commands on this project type.'
    }
    return $dotnet
}

function GetTargetFramework($projectDir) {
    $targetProjectJson = Join-Path $projectDir project.json
    try {
        Write-Debug "Reading $targetProjectJson"
        $project = Get-Content $targetProjectJson -Raw | ConvertFrom-Json
    } catch {
        Write-Verbose $_.Exception.Message
        throw "Invalid JSON file in '$targetProjectJson'"
    }

    $frameworks = $project.frameworks | Get-Member -MemberType NoteProperty | % Name
    if ($frameworks -is 'String') {
        # when there is one framework listed
        return $frameworks
    }
    $choices = [System.Management.Automation.Host.ChoiceDescription[]]( $frameworks | % {$i=0} { new-object System.Management.Automation.Host.ChoiceDescription "Option &$i`: $_"; $i++} )
    $choice = $Host.Ui.PromptForChoice('Multiple target frameworks available', 'Which framework should the command use?', $choices, 0)
    return $frameworks[$choice]
}

function GetDotNetArguments($startupProject, $outputFileName) {
    #TODO use more CPS APIs when/if they become available

    $startupProjectDir = GetProperty $startupProject.Properties FullPath
    $tfm = GetTargetFramework $startupProjectDir
    Write-Verbose "Using framework '$tfm'"
    # only returns part of the actual output path
    $outputPath = Join-Path $startupProjectDir (GetProperty $startupProject.ConfigurationManager.ActiveConfiguration.Properties OutputPath)

    $config = $startupProject.ConfigurationManager.ActiveConfiguration.ConfigurationName

    # TODO get the actual output file from VS when/if CPS API's become available
    $assemblyName = GetProperty $startupProject.Properties AssemblyName

    $arguments = @()
    if ($tfm -like 'net45*' -or $tfm -like 'net46*') {
        # TODO get the actual output file from VS when/if CPS API's become available
        $startupOutputFileName = "$assemblyName.exe"
        #TODO determine if desktop app is x86 or has a different runtimes
        $outputPath = Join-Path $outputPath "$config/$tfm/win7-x64/"
        $exe = Join-Path $PSScriptRoot 'net451/ef.exe'
    } elseif ($tfm -eq 'netcoreapp1.0') {
        #TODO handle self-contained apps

        $outputPath = Join-Path $outputPath "$config/$tfm"
        $exe = GetDotNet
        $arguments += 'exec'
        $arguments += '--additionalprobingpath', "$env:USERPROFILE/.nuget/packages"
        $arguments += '--depsfile', (Join-Path $outputPath "$assemblyName.deps.json")
        $arguments += '--runtimeconfig', (Join-Path $outputPath "$assemblyName.runtimeconfig.json")
        $arguments += Join-Path $PSScriptRoot 'netcoreapp1.0/ef.dll'
        $startupOutputFileName = "$assemblyName.dll"
    } else {
        throw "Commands could not invoke on target framework '$tfm'.`nCommands on ASP.NET Core and .NET Core projects currently only support .NET Core ('netcoreapp1.0') or .NET Framework (e.g. 'net451') target frameworks."
    }


    if ($startupProjectDir -eq $projectDir) {
        $outputFileName = $startupOutputFileName
    }

    $arguments += '--assembly', (Join-Path $outputPath $outputFileName)
    $arguments += '--startup-assembly', (Join-Path $outputPath $startupOutputFileName)

    Write-Verbose "Using data directory '$outputPath'"
    $arguments += '--data-dir', $outputPath

    return @{
        Arguments = $arguments
        Executable = $exe
        OutputPath = $outputPath
    }
}

function GetCsprojArguments($startupProject, $outputFileName) {
    $startupProjectDir = GetProperty $startupProject.Properties FullPath
    $outputPath = Join-Path $startupProjectDir (GetProperty $startupProject.ConfigurationManager.ActiveConfiguration.Properties OutputPath)
    $startupOutputFileName = GetProperty $startupProject.Properties OutputFileName
    $webConfig = GetProjectItem $startupProject 'Web.Config'
    $appConfig = GetProjectItem $startupProject 'App.Config'
    $dataDirectory = $outputPath
    if ($webConfig) {
        $configurationFile = GetProperty $webConfig.Properties FullPath
        $dataDirectory = Join-Path $startupProjectDir 'App_Data'
    } elseif ($appConfig) {
        $configurationFile = GetProperty $appConfig.Properties FullPath
    }

    $arch = GetProperty $startupProject.ConfigurationManager.ActiveConfiguration.Properties PlatformTarget
    if ($arch -eq 'x86') {
        $exe = Join-Path $PSScriptRoot 'net451/ef.x86.exe'
    } elseif ($arch -eq 'AnyCPU' -or $arch -eq 'x64') {
        $exe = Join-Path $PSScriptRoot 'net451/ef.exe'
    } else {
        throw "Cannot invoke command. The current configuration targets and unsupported architecture '$arch'"
    }

    $arguments += '--assembly', (Join-Path $outputPath $outputFileName)
    $arguments += '--startup-assembly', (Join-Path $outputPath $startupOutputFileName)
    $arguments += '--data-dir', $dataDirectory

    if ($configurationFile -and !(IsUwpProject $startupProject)) {
        $arguments += '--config', $configurationFile
    }

    return @{
        Arguments = $arguments
        Executable = $exe
        OutputPath = $outputPath
        ConfigFile = $configurationFile
    }
}

function ProcessCommonParameters($startupProjectName, $projectName, $contextTypeName, $environment, [switch] $suppressContextOption) {
    $project = GetProject $projectName
    $projectDir = GetProperty $project.Properties FullPath

    if (!$contextTypeName -and $project.ProjectName -eq $EFDefaultParameterValues.ProjectName) {
        $contextTypeName = $EFDefaultParameterValues.ContextTypeName
    }

    $startupProject = GetStartupProject $startupProjectName $project
    $startupProjectDir = GetProperty $startupProject.Properties FullPath

    # Enforce project-type restrictions
    if ((IsUwpProject $startupProject) -and (IsClassLibrary $startupProject)) {
        throw "This command cannot use '$($startupProject.Name)' as the startup project because it is a Univeral Windows class library project. Change the startup project to a Universal Windows application project and run this command again."
    }

    if ((IsDotNetProject $startupProject) -and (IsClassLibrary $startupProject)) {
        throw "Could not invoke this command on the startup project '$($startupProject.Name)'.`nEntity Framework Core does not support commands on class library projects in ASP.NET Core and .NET Core applications."
    }

    if (IsDotNetProject $project) {
        $rootNamespace = GetProperty $project.Properties RootNamespace
        # TODO get the actual output file from VS when/if CPS API's allow
        $outputFileName = GetProperty $project.Properties AssemblyName
        $outputFileName += '.dll'
    } else {
        $outputFileName = GetProperty $project.Properties OutputFileName
        $rootNamespace = GetProperty $project.Properties DefaultNamespace
    }

    if (IsDotNetProject $startupProject) {
        if (!(IsDotNetProject $project)) {
            Write-Warning "This command may fail unless both the targeted project and startup project are ASP.NET Core or .NET Core projects."
        }

        $values = GetDotNetArguments $startupProject $outputFileName
    } else {
        $values = GetCsprojArguments $startupProject $outputFileName
    }

	$arguments = @()
    $arguments += $values.Arguments
    $arguments += '--no-color', '--prefix-output', '--verbose'
    $arguments += '--project-dir', $projectDir
    $arguments += '--content-root-path', $startupProjectDir

    if ($rootNamespace) {
        $arguments += '--root-namespace', $rootNamespace
    }

    $options=@()
    if ($environment) {
        $options += '--environment', $environment
    }
    if ($contextTypeName -and !$suppressContextOption) {
        $options += '--context', $contextTypeName
    }

    return @{
        Project = $project
        StartupProject = $startupProject
        Executable = $values.Executable
        OutputPath = $values.OutputPath
        ConfigFile = $values.ConfigFile
        Arguments = $arguments
        Options = $options
    }
}

function InvokeOperation($commonParams, [switch] $json, [switch] $skipBuild) {
    $project = $commonParams.Project
    $startupProject = $commonParams.StartupProject

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
            throw 'Build failed.'
        }

        Write-Verbose 'Build succeeded.'
    }

    $output = $null

    $arguments = @()

    if (IsUwpProject $startupProject) {
        $arguments += , '--no-appdomain'
        $exeCopied = $true
    }

    $arguments += $commonParams.Arguments
    $arguments += $args
    $arguments += $commonParams.Options
    
    if ($json) {
        $arguments += '--json'
    }

    try {
        $exe = $commonParams.Executable
        if ($exeCopied) {
            Write-Debug "Copying '$($commonParams.Executable)' to '$($commonParams.OutputPath)'"
            Copy-Item $commonParams.Executable $commonParams.OutputPath
            $exeFileName = [IO.Path]::GetFileName($commonParams.Executable)
            $exe = Join-Path $commonParams.OutputPath $exeFileName
            if ($commonParams.ConfigFile) {
                # copy binding redirects
                $dest = Join-Path $commonParams.OutputPath "$exeFileName.config"
                Write-Debug "Copying config file'$($commonParams.ConfigFile)' to '$dest'"
                Copy-Item $commonParams.ConfigFile $dest
            }
        }

        try {
            $intermediatePath = Join-Path (GetProperty $commonParams.StartupProject.Properties FullPath) obj
            $rspFile = Join-Path $intermediatePath 'ef.rsp'
            $exe | Out-File -FilePath $rspFile
            $arguments | Out-File -FilePath $rspFile -Append
        } catch {
            Write-Debug 'Failed to write rsp file'
        }

	    Write-Verbose "Running '$exe'"
        $output = Invoke-Process -Executable $exe -Arguments $arguments -RedirectByPrefix -JsonOutput:$json -ErrorAction SilentlyContinue -ErrorVariable invokeErrors
    
    } finally {
        if ($exeCopied) {
            Write-Debug "Cleaning up '$exe'"
            Remove-Item $exe
        }
    }

    $output | Out-String | Write-Debug

    if ($invokeErrors) {
        $combined = ($invokeErrors | ? { !($_.Exception.Message -like '*non-zero exit code')} | % { $_.Exception.Message }) -join "`n"
        if (!$combined) {
            $lastError = $invokeErrors | Select-Object -Last 1
            if (!$lastError.Exception.Message) {
                throw 'Operation failed with unspecified error'
            }

            throw $lastError.Exception.Message
        }
        throw $combined
    }

    if ($json) {
        return $output | ConvertFrom-Json
    } else {
        return $output
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