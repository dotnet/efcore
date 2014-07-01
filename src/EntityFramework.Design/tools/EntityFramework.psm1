$ErrorActionPreference = "Stop"

$EFDefaultParameterValues = @{
    ProjectName = ''
    ContextName = ''
}

#
# Use-EFContext
#

Register-TabExpansion Use-EFContext @{
    ProjectName = { GetProjectNames }
}

function Use-EFContext {
    [CmdletBinding()]
    param ([Parameter(Mandatory = $true)] [string] $ContextName, [string] $ProjectName)

    $project = GetProject $ProjectName

    # TODO: Validate
    $EFDefaultParameterValues.ContextName = $ContextName
    $EFDefaultParameterValues.ProjectName = $project.ProjectName
}

#
# Add-Migration
#

Register-TabExpansion Add-Migration @{
    ProjectName = { GetProjectNames }
}

function Add-Migration {
    [CmdletBinding()]
    param ([Parameter(Mandatory = $true)] [string] $MigrationName, [string] $ContextName, [string] $ProjectName)

    $normalArgs = NormalizeArgs $ProjectName $ContextName
    $fullPath = GetProperty $normalArgs.Project.Properties FullPath
    $migrationsDir = Join-Path $fullPath Migrations

    $artifacts = InvokeOperation $normalArgs.Project CreateMigration @{
        migrationName = $MigrationName
        contextName = $normalArgs.ContextName
        migrationsDir = [string] $migrationsDir
    }

    $artifacts | %{ $normalArgs.Project.ProjectItems.AddFromFile($_) | Out-Null }

    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    FocusPMC
}

#
# Update-Database
#

Register-TabExpansion Update-Database @{
    ProjectName = { Get-ProjectNames }
}

function Update-Database {
    [CmdletBinding()]
    param ([string] $TargetMigration, [string] $ContextName, [string] $ProjectName, [switch] $Script)

    $normalArgs = NormalizeArgs $ProjectName $ContextName
    
    if (!$Script) {
        InvokeOperation $normalArgs.Project PublishMigration @{
            targetMigration = $TargetMigration
            contextName = $normalArgs.ContextName
        }
    }
    else {
        $sql = InvokeOperation $normalArgs.Project CreateMigrationScript @{
            targetMigration = $TargetMigration
            contextName = $normalArgs.ContextName
        }

        # TODO: New SQL File
        $window = $DTE.ItemOperations.NewFile('General\Text File')
        $textDocument = $window.Document.Object('TextDocument')
        $editPoint = $textDocument.StartPoint.CreateEditPoint();
        $editPoint.Insert($sql);
        FocusPMC
    }
}

#
# (Private Helpers)
#

function GetProjectNames {
    $projects = Get-Project -All

    return @(
        $projects | select -ExpandProperty ProjectName
        $projects | group Name | ? Count -eq 1 | select -ExpandProperty Name
    ) | sort -Unique
}

function NormalizeArgs($projectName, $contextName) {
    if (!$contextName) {
        $projectName = $EFDefaultParameterValues.ProjectName
        $contextName = $EFDefaultParameterValues.ContextName
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

function FocusPMC {
    $componentModel = Get-VSComponentModel
    $powerConsoleWindow = $componentModel.GetService([NuGetConsole.IPowerConsoleWindow])
    $powerConsoleWindow.Show()
}

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
        $targetPath = Join-Path $targetDir $targetFileName

        Write-Verbose "Using assembly '$targetFileName'"

        $executor = $domain.CreateInstanceAndUnwrap(
            $assemblyName,
            $typeName,
            $false,
            0,
            $null,
            @(@{ targetPath = [string] $targetPath }),
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
                ($executor, [MarshalByRefObject] $handler, $arguments),
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
