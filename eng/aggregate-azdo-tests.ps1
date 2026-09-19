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
    HelixJobMonitor = 'Monitor Helix Jobs'
}

# Test runs uploaded by the Helix Job Monitor are named after their target queue.
# Public queue names add ".Open"; normalize that suffix so the same map works for internal builds.
$helixQueueNames = @{
    Helix_Windows = 'Windows.10.Amd64'
    Helix_Windows_SqlServer = 'Windows.11.Amd64.Client'
    Helix_Windows_Arm64 = 'Windows.11.Arm64'
    Helix_Windows_Cosmos = 'Windows.Server2025.Amd64'
    Helix_macOS_x64 = 'OSX.15.Amd64'
    Helix_macOS_ARM64 = 'OSX.15.ARM64'
    Helix_Ubuntu_SqlServer = 'Ubuntu.2204.Amd64.XL@mcr.microsoft.com/dotnet-buildtools/prereqs:ubuntu-22.04-helix-sqlserver-amd64'
    Helix_Ubuntu_Cosmos = 'Ubuntu.2204.Amd64.XL'
    Helix_Ubuntu = 'Ubuntu.2204.Amd64'
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
        | Where-Object { $_.type -eq 'Job' -and ($_.name -eq $jobName -or $_.name -eq $displayName) }
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

function Get-AzureDevOpsApiContext
{
    if ([string]::IsNullOrEmpty($env:SYSTEM_ACCESSTOKEN))
    {
        throw 'SYSTEM_ACCESSTOKEN is required to get Helix test results.'
    }

    if ([string]::IsNullOrEmpty($env:SYSTEM_COLLECTIONURI) -or
        [string]::IsNullOrEmpty($env:SYSTEM_TEAMPROJECT))
    {
        throw 'SYSTEM_COLLECTIONURI and SYSTEM_TEAMPROJECT are required to get Helix test results.'
    }

    $project = [Uri]::EscapeDataString($env:SYSTEM_TEAMPROJECT)
    @{
        ApiBaseUri = "$($env:SYSTEM_COLLECTIONURI.TrimEnd('/'))/$project/_apis"
        Headers = @{ Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN" }
    }
}

function Get-NormalizedHelixQueueName([string]$queueName)
{
    return $queueName -replace '\.Open(?=@|$)', ''
}

function Invoke-AzureDevOpsRestMethod([string]$uri, [hashtable]$headers)
{
    $maxAttempts = 3

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++)
    {
        try
        {
            return Invoke-RestMethod `
                -Uri $uri `
                -Headers $headers `
                -ErrorAction Stop
        }
        catch
        {
            $statusCode = 0
            if ($null -ne $_.Exception.Response)
            {
                $statusCode = [int]$_.Exception.Response.StatusCode
            }

            if ($attempt -eq $maxAttempts -or $statusCode -notin @(408, 429, 500, 502, 503, 504))
            {
                throw
            }

            $delay = [Math]::Pow(2, $attempt)
            Write-Warning "Azure DevOps request failed with HTTP $statusCode. Retrying in $delay seconds."
            Start-Sleep -Seconds $delay
        }
    }
}

function Get-AzureDevOpsTestRuns([int]$buildId, [hashtable]$apiContext)
{
    $buildUri = [Uri]::EscapeDataString("vstfs:///Build/Build/$buildId")
    $pageSize = 1000
    $skip = 0
    $testRuns = @()

    do
    {
        $uri = "$($apiContext.ApiBaseUri)/test/runs?buildUri=$buildUri&%24top=$pageSize&%24skip=$skip&api-version=7.1"
        $response = Invoke-AzureDevOpsRestMethod $uri $apiContext.Headers
        $page = @($response.value)
        $testRuns += $page
        $skip += $page.Count
    }
    while ($page.Count -eq $pageSize)

    return $testRuns
}

function Set-HelixJobResults(
    [hashtable]$resultsByJob,
    [string[]]$jobNames,
    [int]$buildId)
{
    $helixJobNames = @($jobNames | Where-Object { $_ -like 'Helix_*' })
    if ($helixJobNames.Count -eq 0)
    {
        return
    }

    foreach ($jobName in $helixJobNames)
    {
        if (-not $resultsByJob.ContainsKey($jobName))
        {
            throw "Missing result for Helix job '$jobName'."
        }

        if (-not $helixQueueNames.ContainsKey($jobName))
        {
            throw "Missing Helix queue mapping for job '$jobName'."
        }
    }

    $apiContext = Get-AzureDevOpsApiContext
    $testRuns = @(Get-AzureDevOpsTestRuns $buildId $apiContext)

    foreach ($jobName in $helixJobNames)
    {
        if ($resultsByJob[$jobName] -eq 'Skipped')
        {
            continue
        }

        if ($resultsByJob[$jobName] -notin @('Succeeded', 'SucceededWithIssues'))
        {
            Write-Warning "Helix submission job '$jobName' did not succeed: $($resultsByJob[$jobName])."
            $resultsByJob[$jobName] = 'Failed'
            continue
        }

        $queueName = $helixQueueNames[$jobName]
        # Job Monitor retries create a new run with the same queue name containing only the retried work items.
        $run = @($testRuns
            | Where-Object { (Get-NormalizedHelixQueueName $_.name) -eq $queueName }
            | Sort-Object id -Descending)[0]

        if ($null -eq $run)
        {
            Write-Warning "No completed Helix test run was found for job '$jobName' and queue '$queueName'."
            $resultsByJob[$jobName] = 'Failed'
            continue
        }

        if ($run.state -ne 'Completed')
        {
            throw "Helix test run $($run.id) for job '$jobName' did not complete; its state is '$($run.state)'."
        }

        $attachmentsResponse = Invoke-AzureDevOpsRestMethod `
            "$($apiContext.ApiBaseUri)/test/runs/$($run.id)/attachments?api-version=7.1" `
            $apiContext.Headers
        $failedWorkItemsAttachment = @($attachmentsResponse.value
            | Where-Object { $_.fileName -eq 'helix-failed-workitems.json' })

        if ($failedWorkItemsAttachment.Count -gt 0)
        {
            $resultsByJob[$jobName] = 'Failed'
        }
        else
        {
            $resultsByJob[$jobName] = 'Succeeded'
        }

        Write-Host "  $jobName ($($run.name)): $($resultsByJob[$jobName])"
    }
}

$jobAttempt = 1
[void][int]::TryParse($env:SYSTEM_JOBATTEMPT, [ref]$jobAttempt)
$stageAttempt = 1
[void][int]::TryParse($env:SYSTEM_STAGEATTEMPT, [ref]$stageAttempt)
$buildId = 0
if (-not [int]::TryParse($env:BUILD_BUILDID, [ref]$buildId) -or $buildId -le 0)
{
    throw "BUILD_BUILDID must contain a valid build ID; received '$env:BUILD_BUILDID'."
}

Set-HelixJobResults $JobResults @($helixQueueNames.Keys) $buildId
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
    $definitionId = 0
    if (-not [int]::TryParse($env:SYSTEM_DEFINITIONID, [ref]$definitionId) -or $definitionId -le 0)
    {
        throw "SYSTEM_DEFINITIONID must contain a valid pipeline definition ID; received '$env:SYSTEM_DEFINITIONID'."
    }

    $project = [Uri]::EscapeDataString($env:SYSTEM_TEAMPROJECT)
    $buildsUri = "$($env:SYSTEM_COLLECTIONURI.TrimEnd('/'))/$project/_apis/build/builds"
    $headers = @{ Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN" }
    $queueBody = @{
        definition = @{ id = $definitionId }
        sourceBranch = $env:BUILD_SOURCEBRANCH
        sourceVersion = $env:BUILD_SOURCEVERSION
        # The REST model requires string values; jobs is a JSON array consumed by the pipeline's string parameter.
        templateParameters = @{ jobs = $jobsParameter }
    } | ConvertTo-Json -Depth 4

    Write-Host "Retrying jobs: $($jobsToRetry -join ', ')"
    $retryBuild = Invoke-RestMethod -Uri "$buildsUri`?api-version=7.1" -Method Post -Headers $headers -ContentType 'application/json' -Body $queueBody
    if ($null -eq $retryBuild.id)
    {
        throw 'The retry build response did not contain a build ID.'
    }

    $retryBuildUrl = $retryBuild._links.web.href
    if ([string]::IsNullOrEmpty($retryBuildUrl))
    {
        $retryBuildUrl = "$($env:SYSTEM_COLLECTIONURI.TrimEnd('/'))/$project/_build/results?buildId=$($retryBuild.id)"
    }

    Write-Host "Retry build: $retryBuildUrl"

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

    $helixJobsToRetry = @($jobsToRetry | Where-Object { $_ -like 'Helix_*' })
    if ($helixJobsToRetry.Count -gt 0)
    {
        $monitorRecord = Get-JobRecord $timeline 'HelixJobMonitor'
        if ($null -eq $monitorRecord)
        {
            throw 'Could not find the Helix Job Monitor timeline record for the retry build.'
        }

        Set-HelixJobResults $JobResults $helixJobsToRetry $retryBuild.id
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
    }
}

if ($failedGroups.Count -gt 0)
{
    throw "No jobs succeeded for group(s): $($failedGroups -join ', ')."
}

Write-Host 'Group validation passed.'