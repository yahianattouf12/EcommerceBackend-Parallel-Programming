<#
Stress / load test script for the ECommerceBackend API.
Fires a configurable number of concurrent HTTP requests against a target endpoint
using async HttpClient tasks (true concurrency, not a sequential loop), then
reports total/success/failed counts, average response time, and a post-test
stability check.

Usage examples:
  .\load-test.ps1
  .\load-test.ps1 -Url "http://localhost:5202/api/products/update-stock-optimistic?productId=1&quantity=1" -Method POST -TotalRequests 300 -Concurrency 50
  .\load-test.ps1 -Url "http://localhost:5202/api/orders/checkout" -Method POST -TotalRequests 100 -Concurrency 20
#>

param(
    [string]$Url = "http://localhost:5202/api/products/update-stock-optimistic?productId=1&quantity=1",
    [ValidateSet("GET", "POST")]
    [string]$Method = "POST",
    [int]$TotalRequests = 300,
    [int]$Concurrency = 50,
    [string]$CsvOutput = "stress-test-results.csv"
)

Add-Type -AssemblyName System.Net.Http

$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(30)

$allResults = New-Object System.Collections.Generic.List[object]
$batches = [Math]::Ceiling($TotalRequests / $Concurrency)

Write-Host "=== Stress Test Starting ===" -ForegroundColor Cyan
Write-Host "Target URL     : $Url"
Write-Host "Method         : $Method"
Write-Host "Total Requests : $TotalRequests"
Write-Host "Concurrency    : $Concurrency"
Write-Host "Batches        : $batches"
Write-Host ""

$swTotal = [System.Diagnostics.Stopwatch]::StartNew()

for ($b = 0; $b -lt $batches; $b++) {
    $remaining = $TotalRequests - ($b * $Concurrency)
    $batchSize = [Math]::Min($Concurrency, $remaining)

    # كل طلب هنا Task غير متزامن (async) يبدأ فوراً - كل دفعة تنطلق كلها معاً بنفس اللحظة، هذا هو الضغط المتزامن الحقيقي
    $tasks = New-Object System.Collections.Generic.List[System.Threading.Tasks.Task[System.Net.Http.HttpResponseMessage]]
    $stopwatches = New-Object System.Collections.Generic.List[System.Diagnostics.Stopwatch]

    for ($i = 0; $i -lt $batchSize; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        if ($Method -eq "POST") {
            $emptyBody = [System.Net.Http.StringContent]::new("")
            $task = $client.PostAsync($Url, $emptyBody)
        }
        else {
            $task = $client.GetAsync($Url)
        }
        $tasks.Add($task)
        $stopwatches.Add($sw)
    }

    # ننتظر اكتمال كل الطلبات في هذه الدفعة (لكنها كلها كانت تعمل بالتوازي قبل هذا السطر)
    try {
        [System.Threading.Tasks.Task]::WaitAll($tasks.ToArray())
    }
    catch {
        # WaitAll يرمي AggregateException لو فشل أي Task - نتجاهلها هنا ونتعامل مع كل نتيجة بالأسفل
    }

    for ($i = 0; $i -lt $tasks.Count; $i++) {
        $stopwatches[$i].Stop()
        $t = $tasks[$i]

        if ($t.IsFaulted) {
            $allResults.Add([PSCustomObject]@{
                StatusCode = 0
                Success    = $false
                ElapsedMs  = $stopwatches[$i].Elapsed.TotalMilliseconds
                Error      = $t.Exception.InnerException.Message
            })
        }
        else {
            $resp = $t.Result
            $allResults.Add([PSCustomObject]@{
                StatusCode = [int]$resp.StatusCode
                Success    = $resp.IsSuccessStatusCode
                ElapsedMs  = $stopwatches[$i].Elapsed.TotalMilliseconds
                Error      = $null
            })
        }
    }

    Write-Host "Batch $($b + 1)/$batches done ($($allResults.Count)/$TotalRequests requests so far)"
}

$swTotal.Stop()

# ---- Metrics ----
$total = $allResults.Count
$successCount = ($allResults | Where-Object { $_.Success }).Count
$failedCount = $total - $successCount
$avgMs = ($allResults | Measure-Object -Property ElapsedMs -Average).Average
$maxMs = ($allResults | Measure-Object -Property ElapsedMs -Maximum).Maximum
$minMs = ($allResults | Measure-Object -Property ElapsedMs -Minimum).Minimum

# فحص الاستقرار: هل السيرفر لا زال يرد بشكل سليم بعد نهاية الضغط؟ (نستخدم GET /api/products كفحص خفيف)
$stable = $false
try {
    $baseUri = [System.Uri]::new($Url)
    $healthUrl = "$($baseUri.Scheme)://$($baseUri.Authority)/api/products"
    $healthCheck = $client.GetAsync($healthUrl).Result
    $stable = $healthCheck.IsSuccessStatusCode
}
catch {
    $stable = $false
}

Write-Host ""
Write-Host "=== Stress Test Results ===" -ForegroundColor Cyan
Write-Host "Total Requests      : $total"
Write-Host "Successful Requests : $successCount" -ForegroundColor Green
Write-Host "Failed Requests     : $failedCount" -ForegroundColor $(if ($failedCount -gt 0) { "Yellow" } else { "Green" })
Write-Host "Average Response    : $([math]::Round($avgMs, 2)) ms"
Write-Host "Min / Max Response  : $([math]::Round($minMs, 2)) ms / $([math]::Round($maxMs, 2)) ms"
Write-Host "Total Test Duration : $([math]::Round($swTotal.Elapsed.TotalSeconds, 2)) s"
Write-Host "Server Stable After Test : $stable" -ForegroundColor $(if ($stable) { "Green" } else { "Red" })

$allResults | Export-Csv -Path $CsvOutput -NoTypeInformation
Write-Host ""
Write-Host "Per-request details saved to: $CsvOutput"

$client.Dispose()
