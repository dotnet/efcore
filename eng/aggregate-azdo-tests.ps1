[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [hashtable]$JobResults
)

# Aggregate redundant test jobs into logical groups, and on validation retries run only the jobs needed by failed groups.
# Azure DevOps timeline records expose display names, while the pipeline and group definitions use job IDs.
$jobDisplayNames = @{
    Windows = 'Windows'
    Windows_SqlServer = 'Windows SQL Server'
    macOS = 'macOS'
    Linux = 'Linux'
    Helix_Windows = 'Helix Windows'
    Helix_Windows_SqlServer = 'Helix Windows SQL Server'
    Helix_Windows_Arm64 = 'Helix Windows ARM64'
    Helix_Windows_Cosmos = 'Helix Windows Cosmos'
    Helix_macOS_x64 = 'Helix macOS x64'
    Helix_macOS_ARM64 = 'Helix macOS ARM64'
    Helix_Ubuntu_SqlServer = 'Helix Ubuntu SQL Server'
    Helix_Ubuntu_Cosmos = 'Helix Ubuntu Cosmos'
    Helix_Ubuntu = 'Helix Ubuntu'
}

# A group succeeds when at least one of its jobs succeeds. Jobs may participate in multiple groups.
$groupJobs = @{
    Windows = @('Windows', 'Helix_Windows')
    Linux = @('Linux', 'Helix_Ubuntu')
    MacOS = @('macOS', 'Helix_macOS_x64')
    Arm64 = @('Helix_Windows_Arm64', 'Helix_macOS_ARM64')
    Cosmos = @('Helix_Windows_Cosmos', 'Helix_Ubuntu_Cosmos')
    SqlServer = @('Windows_SqlServer', 'Helix_Windows_SqlServer', 'Helix_Ubuntu_SqlServer')
}

function Get-JobRecord($timeline, [string]$jobName)
{
    if (-not $jobDisplayNames.ContainsKey($jobName))
    {
        throw "Unknown jobName '$jobName'."
    }

    $displayName = $jobDisplayNames[$jobName]
    @($timeline.records
        | Where-Object { $_.type -eq 'Job' -and $_.name -eq $displayName }
        | Sort-Object attempt -Descending)[0]
}

function Get-FailedGroups([hashtable]$resultsByJob)
{
    $failed = @()

    foreach ($groupName in $groupJobs.Keys)
    {
        foreach ($jobName in $groupJobs[$groupName])
        {
            if (-not $resultsByJob.ContainsKey($jobName))
            {
                throw "Missing result for job '$jobName' in group '$groupName'."
            }
        }

        $results = @($groupJobs[$groupName] | ForEach-Object { $resultsByJob[$_] })
        $ran = @($results | Where-Object { $_ -ne 'Skipped' })

        if ($ran.Count -gt 0 -and $ran -notcontains 'Succeeded')
        {
            $failed += $groupName
        }
    }

    return $failed
}

$jobAttempt = 1
[void][int]::TryParse($env:SYSTEM_JOBATTEMPT, [ref]$jobAttempt)
$stageAttempt = 1
[void][int]::TryParse($env:SYSTEM_STAGEATTEMPT, [ref]$stageAttempt)
$failedGroups = @(Get-FailedGroups $JobResults)

# Retrying validation queues a child build containing the distinct jobs needed by all failed groups.
if (($jobAttempt -gt 1 -or $stageAttempt -gt 1) -and $failedGroups.Count -gt 0)
{
    if ([string]::IsNullOrEmpty($env:SYSTEM_ACCESSTOKEN))
    {
        throw 'SYSTEM_ACCESSTOKEN is required to retry build jobs.'
    }

    $jobsToRetry = @($failedGroups | ForEach-Object { $groupJobs[$_] } | Select-Object -Unique)
    $jobsParameter = ConvertTo-Json -InputObject $jobsToRetry -Compress
    $project = [Uri]::EscapeDataString($env:SYSTEM_TEAMPROJECT)
    $buildsUri = "$($env:SYSTEM_COLLECTIONURI.TrimEnd('/'))/$project/_apis/build/builds"
    $headers = @{ Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN" }
    $queueBody = @{
        definition = @{ id = [int]$env:BUILD_DEFINITIONID }
        sourceBranch = $env:BUILD_SOURCEBRANCH
        sourceVersion = $env:BUILD_SOURCEVERSION
        templateParameters = @{ jobs = $jobsParameter }
    } | ConvertTo-Json -Depth 4

    Write-Host "Retrying jobs: $($jobsToRetry -join ', ')"
    $retryBuild = Invoke-RestMethod -Uri "$buildsUri`?api-version=7.1" -Method Post -Headers $headers -ContentType 'application/json' -Body $queueBody
    if ($null -eq $retryBuild.id)
    {
        throw 'The retry build response did not contain a build ID.'
    }

    $deadline = [DateTime]::UtcNow.AddMinutes(190)
    # Wait for the child build so its final job results can replace the original failed results.
    do
    {
        Start-Sleep -Seconds 60
        $retryBuild = Invoke-RestMethod -Uri "$buildsUri/$($retryBuild.id)?api-version=7.1" -Headers $headers
    }
    while ($retryBuild.status -ne 'completed' -and [DateTime]::UtcNow -lt $deadline)

    if ($retryBuild.status -ne 'completed')
    {
        throw "Timed out waiting for retry build $($retryBuild.id)."
    }

    $timeline = Invoke-RestMethod -Uri "$buildsUri/$($retryBuild.id)/timeline?api-version=7.1" -Headers $headers
    # Merge only retried jobs; results from groups that already passed remain unchanged.
    foreach ($jobName in $jobsToRetry)
    {
        $record = Get-JobRecord $timeline $jobName
        if ($null -eq $record)
        {
            throw "Could not find timeline record for retried job '$jobName'."
        }

        $JobResults[$jobName] = $record.result
    }

    $failedGroups = @(Get-FailedGroups $JobResults)
}

foreach ($groupName in $groupJobs.Keys)
{
    $results = @($groupJobs[$groupName] | ForEach-Object { $JobResults[$_] })
    Write-Host "$groupName results: $($results -join ', ')"

    $ran = @($results | Where-Object { $_ -ne 'Skipped' })
    if ($ran.Count -eq 0)
    {
        Write-Host '  -> all jobs skipped, treating group as successful'
        continue
    }

    if ($ran -contains 'Succeeded')
    {
        continue
    }
}

if ($failedGroups.Count -gt 0)
{
    throw "No jobs succeeded for group(s): $($failedGroups -join ', ')."
}

Write-Host 'Group validation passed.'