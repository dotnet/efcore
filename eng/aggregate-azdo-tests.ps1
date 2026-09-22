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

# A group succeeds when at least one of its jobs succeeds. Jobs may participate in multiple groups.
$groupJobs = @{
    Windows = @('Windows', 'Helix_Windows')
    Linux = @('Linux', 'Helix_Ubuntu')
    MacOS = @('macOS', 'Helix_macOS_x64')
    Arm64 = @('Helix_Windows_Arm64', 'Helix_macOS_ARM64')
    Cosmos = @('Helix_Windows_Cosmos', 'Helix_Ubuntu_Cosmos')
    SqlServer = @('Windows_SqlServer', 'Helix_Windows_SqlServer', 'Helix_Ubuntu_SqlServer')
}

$helixJobNames = @($groupJobs.Values
    | ForEach-Object { $_ }
    | Where-Object { $_ -like 'Helix_*' }
    | Select-Object -Unique)

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
    $collectionUri = [Uri]$env:SYSTEM_COLLECTIONURI
    $vstmrHost = if ($collectionUri.Host -eq 'dev.azure.com')
    {
        'vstmr.dev.azure.com'
    }
    elseif ($collectionUri.Host.EndsWith('.visualstudio.com'))
    {
        $collectionUri.Host.Insert($collectionUri.Host.IndexOf('.'), '.vstmr')
    }
    else
    {
        $collectionUri.Host
    }

    $vstmrCollectionUri = [UriBuilder]::new($collectionUri)
    $vstmrCollectionUri.Host = $vstmrHost

    @{
        ApiBaseUri = "$($env:SYSTEM_COLLECTIONURI.TrimEnd('/'))/$project/_apis"
        VstmrApiBaseUri = "$($vstmrCollectionUri.Uri.ToString().TrimEnd('/'))/$project/_apis"
        Headers = @{ Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN" }
    }
}

function Invoke-RestMethodWithRetry(
    [string]$uri,
    [hashtable]$headers,
    [string]$serviceName)
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
            Write-Warning "$serviceName request failed with HTTP $statusCode. Retrying in $delay seconds."
            Start-Sleep -Seconds $delay
        }
    }
}

function Invoke-AzureDevOpsRestMethod([string]$uri, [hashtable]$headers)
{
    return Invoke-RestMethodWithRetry $uri $headers 'Azure DevOps'
}

function Invoke-HelixRestMethod([string]$uri)
{
    $headers = @{}
    if (-not [string]::IsNullOrEmpty($env:HELIX_ACCESSTOKEN))
    {
        $headers.Authorization = "token $env:HELIX_ACCESSTOKEN"
    }

    return Invoke-RestMethodWithRetry $uri $headers 'Helix'
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

function Get-HelixJobNameFromTestRun([int]$runId, [hashtable]$apiContext)
{
    $run = Invoke-AzureDevOpsRestMethod `
        "$($apiContext.VstmrApiBaseUri)/testresults/runs/$runId`?includeTags=true&api-version=7.1-preview.1" `
        $apiContext.Headers
    $tag = @($run.tags | Where-Object { $_.name -match '^helixjob(?<jobId>[0-9a-f]{32})$' })[0]

    if ($null -eq $tag)
    {
        return $null
    }

    return [Guid]::ParseExact($tag.name.Substring('helixjob'.Length), 'N').ToString('D')
}

function Get-HelixTestRuns([int]$buildId, [hashtable]$apiContext)
{
    $testRuns = @(Get-AzureDevOpsTestRuns $buildId $apiContext)
    $helixTestRuns = @()

    foreach ($testRun in $testRuns)
    {
        if ($testRun.state -ne 'Completed')
        {
            continue
        }

        $helixJobName = Get-HelixJobNameFromTestRun $testRun.id $apiContext
        if ([string]::IsNullOrEmpty($helixJobName))
        {
            continue
        }

        $helixJob = Invoke-HelixRestMethod "https://helix.dot.net/api/2019-06-17/jobs/$helixJobName"
        $phaseName = $helixJob.Properties.'System.PhaseName'
        if ([string]::IsNullOrEmpty($phaseName))
        {
            Write-Warning "Helix job '$helixJobName' for test run $($testRun.id) has no System.PhaseName property."
            continue
        }

        $stageAttempt = 0
        [void][int]::TryParse($helixJob.Properties.'System.StageAttempt', [ref]$stageAttempt)
        $jobAttempt = 0
        [void][int]::TryParse($helixJob.Properties.'System.JobAttempt', [ref]$jobAttempt)

        $helixTestRuns += @{
            HelixJobName = $helixJobName
            JobAttempt = $jobAttempt
            LogicalJobName = $helixJob.Properties.jobName
            PhaseName = $phaseName
            QueueId = $helixJob.QueueId
            StageAttempt = $stageAttempt
            TestRun = $testRun
        }
    }

    return $helixTestRuns
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
    }

    if (-not $resultsByJob.ContainsKey('HelixJobMonitor'))
    {
        throw "Missing result for job 'HelixJobMonitor'."
    }

    $apiContext = Get-AzureDevOpsApiContext
    $helixTestRuns = @(Get-HelixTestRuns $buildId $apiContext)
    $missingRuns = @()

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

        $phaseRuns = @($helixTestRuns | Where-Object { $_.PhaseName -eq $jobName })

        if ($phaseRuns.Count -eq 0)
        {
            $missingRuns += $jobName
            continue
        }

        $latestStageAttempt = ($phaseRuns.StageAttempt | Measure-Object -Maximum).Maximum
        $phaseRuns = @($phaseRuns | Where-Object { $_.StageAttempt -eq $latestStageAttempt })
        $latestJobAttempt = ($phaseRuns.JobAttempt | Measure-Object -Maximum).Maximum
        $phaseRuns = @($phaseRuns | Where-Object { $_.JobAttempt -eq $latestJobAttempt })

        foreach ($runInfo in $phaseRuns)
        {
            $runInfo['StreamName'] = if (-not [string]::IsNullOrEmpty($runInfo.LogicalJobName))
            {
                "$($runInfo.QueueId)|$($runInfo.LogicalJobName)"
            }
            elseif (-not [string]::IsNullOrEmpty($runInfo.QueueId))
            {
                $runInfo.QueueId
            }
            else
            {
                $runInfo.HelixJobName
            }
        }

        # A phase may submit multiple logical Helix jobs. Monitor retries preserve each stream's
        # queue and logical job name, and create a newer test run for that stream.
        $authoritativeRuns = @($phaseRuns
            | Group-Object StreamName
            | ForEach-Object { @($_.Group | Sort-Object { $_.TestRun.id } -Descending)[0] })
        $resultsByJob[$jobName] = 'Succeeded'

        foreach ($runInfo in $authoritativeRuns)
        {
            $run = $runInfo.TestRun
            $attachmentsResponse = Invoke-AzureDevOpsRestMethod `
                "$($apiContext.ApiBaseUri)/test/runs/$($run.id)/attachments?api-version=7.1" `
                $apiContext.Headers
            $failedWorkItemsAttachment = @($attachmentsResponse.value
                | Where-Object { $_.fileName -eq 'helix-failed-workitems.json' })

            if ($failedWorkItemsAttachment.Count -gt 0)
            {
                $resultsByJob[$jobName] = 'Failed'
            }

            Write-Host "  $jobName ($($run.name), Helix job $($runInfo.HelixJobName)): $($resultsByJob[$jobName])"
        }
    }

    if ($missingRuns.Count -gt 0)
    {
        if ($resultsByJob.HelixJobMonitor -eq 'Succeeded')
        {
            Write-Warning "No completed monitor test run was found for $($missingRuns -join ', '), but the Helix Job Monitor succeeded. Treating those Helix jobs as successful."
            foreach ($jobName in $missingRuns)
            {
                $resultsByJob[$jobName] = 'Succeeded'
            }
        }
        else
        {
            throw "The Helix Job Monitor result was '$($resultsByJob.HelixJobMonitor)' and it did not publish completed test runs for: $($missingRuns -join ', ')."
        }
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

Set-HelixJobResults $JobResults $helixJobNames $buildId
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

        $JobResults.HelixJobMonitor = $monitorRecord.result
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