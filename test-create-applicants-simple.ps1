<#
.SYNOPSIS
    Simple bulk test script for creating applicants via API endpoint

.DESCRIPTION
    This script sends sequential POST requests to the create applicant endpoint
    with unique mocked data to test performance and database insertion capability.

.PARAMETER ApiBaseUrl
    The base URL of the API (default: http://localhost:5259/api)

.PARAMETER TotalRequests
    Total number of requests to send (default: 100)

.EXAMPLE
    .\test-create-applicants-simple.ps1
    .\test-create-applicants-simple.ps1 -ApiBaseUrl "http://localhost:5000/api" -TotalRequests 50
#>

param(
    [string]$ApiBaseUrl = "http://localhost:5259/api",
    [int]$TotalRequests = 100
)

# Countries list for realistic data
$countries = @(
    "United States", "Canada", "United Kingdom", "Germany", "France", "Italy", "Spain", "Australia", 
    "Japan", "China", "India", "Brazil", "Mexico", "Argentina", "Egypt", "South Africa", "Nigeria",
    "Kenya", "Morocco", "United Arab Emirates", "Saudi Arabia", "Turkey", "Russia", "Poland",
    "Netherlands", "Belgium", "Switzerland", "Austria", "Sweden", "Norway", "Denmark", "Finland"
)

# First names pool
$firstNames = @(
    "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth",
    "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen",
    "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Betty", "Anthony", "Dorothy", "Mark", "Sandra",
    "Donald", "Ashley", "Steven", "Kimberly", "Paul", "Donna", "Andrew", "Emily", "Joshua", "Michelle",
    "Kenneth", "Carol", "Kevin", "Amanda", "Brian", "Melissa", "George", "Deborah", "Edward", "Stephanie"
)

# Last names pool
$lastNames = @(
    "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
    "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
    "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
    "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
    "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
)

# Street names pool
$streetNames = @(
    "Main Street", "Oak Avenue", "Pine Road", "Elm Drive", "Cedar Lane", "Maple Street", "Birch Avenue",
    "Willow Road", "Park Drive", "First Street", "Second Avenue", "Third Road", "Fourth Drive", "Fifth Lane",
    "Washington Street", "Lincoln Avenue", "Jefferson Road", "Madison Drive", "Adams Lane", "Monroe Street",
    "Broadway", "Market Street", "Church Road", "School Drive", "Library Lane", "Parkway", "Boulevard",
    "Circle", "Court", "Place", "Way", "Trail", "Path", "Terrace", "Gardens", "Heights", "Valley"
)

# Email domains
$emailDomains = @("gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "company.com", "mail.com")

# Function to generate random applicant data
function Generate-RandomApplicantData($index) {
    $firstName = $firstNames[(Get-Random -Minimum 0 -Maximum $firstNames.Count)]
    $lastName = $lastNames[(Get-Random -Minimum 0 -Maximum $lastNames.Count)]
    $country = $countries[(Get-Random -Minimum 0 -Maximum $countries.Count)]
    $street = $streetNames[(Get-Random -Minimum 0 -Maximum $streetNames.Count)]
    $houseNumber = Get-Random -Minimum 100 -Maximum 9999
    $emailDomain = $emailDomains[(Get-Random -Minimum 0 -Maximum $emailDomains.Count)]
    
    $age = Get-Random -Minimum 20 -Maximum 51
    $phoneSuffix = Get-Random -Minimum 100000000 -Maximum 999999999
    
    $appliedDate = (Get-Date).AddDays(-(Get-Random -Minimum 0 -Maximum 365)).ToString("yyyy-MM-dd")
    
    return @{
        Name = $firstName
        FamilyName = $lastName
        Address = "$houseNumber $street"
        EmailAddress = "$firstName.$lastName.$index@$emailDomain".ToLower()
        Phone = "+20$phoneSuffix"
        Age = $age
        CountryOfOrigin = $country
        AppliedDate = $appliedDate
        Hired = $false
    }
}

# Function to create a single applicant
function Create-Applicant($applicantData, $requestNumber) {
    $endpoint = "$ApiBaseUrl/applicants"
    $body = $applicantData | ConvertTo-Json
    
    Write-Host "Request $requestNumber`: Creating applicant - $($applicantData.Name) $($applicantData.FamilyName)" -ForegroundColor Yellow
    
    $requestStart = Get-Date
    
    try {
        $response = Invoke-RestMethod -Uri $endpoint -Method POST -Body $body -ContentType "application/json" -TimeoutSec 30
        $requestEnd = Get-Date
        $responseTime = ($requestEnd - $requestStart).TotalMilliseconds
        
        Write-Host "✅ Success - Applicant ID: $response (Response Time: $([Math]::Round($responseTime, 2)) ms)" -ForegroundColor Green
        
        return @{
            Success = $true
            ApplicantId = $response
            Data = $applicantData
            ResponseTime = $responseTime
            Error = $null
        }
    }
    catch {
        $requestEnd = Get-Date
        $responseTime = ($requestEnd - $requestStart).TotalMilliseconds
        
        $errorMessage = $_.Exception.Message
        if ($_.Exception.Response) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $reader.BaseStream.Position = 0
                $reader.DiscardBufferedData()
                $errorMessage = $reader.ReadToEnd()
                $reader.Close()
            }
            catch {
                # Fallback error handling
            }
        }
        
        Write-Host "❌ Failed - Error: $errorMessage (Response Time: $([Math]::Round($responseTime, 2)) ms)" -ForegroundColor Red
        
        return @{
            Success = $false
            ApplicantId = $null
            Data = $applicantData
            ResponseTime = $responseTime
            Error = $errorMessage
        }
    }
}

# Function to get current database count
function Get-DatabaseCount {
    try {
        $countEndpoint = "$ApiBaseUrl/applicants?page=1&pageSize=1"
        $response = Invoke-RestMethod -Uri $countEndpoint -Method GET -TimeoutSec 30
        return $response.totalCount
    }
    catch {
        Write-Host "Warning: Could not fetch database count - $($_.Exception.Message)" -ForegroundColor Yellow
        return -1
    }
}

# Main execution
Write-Host "=== Simple Bulk Applicant Creation Test ===" -ForegroundColor Cyan
Write-Host "API Base URL: $ApiBaseUrl" -ForegroundColor Yellow
Write-Host "Total Requests: $TotalRequests" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan

# Get initial count
$initialCount = Get-DatabaseCount
Write-Host "Initial database count: $initialCount" -ForegroundColor Green

$startTime = Get-Date
$results = @()
$successCount = 0
$failureCount = 0
$totalResponseTime = 0

# Process requests sequentially
for ($i = 1; $i -le $TotalRequests; $i++) {
    $applicantData = Generate-RandomApplicantData($i)
    $result = Create-Applicant -applicantData $applicantData -requestNumber $i
    
    $results += $result
    
    if ($result.Success) {
        $successCount++
    }
    else {
        $failureCount++
    }
    
    $totalResponseTime += $result.ResponseTime
    
    # Small delay between requests to avoid overwhelming the server
    if ($i -lt $TotalRequests) {
        Start-Sleep -Milliseconds 100
    }
}

$endTime = Get-Date
$totalDuration = ($endTime - $startTime).TotalSeconds
$averageResponseTime = if ($TotalRequests -gt 0) { $totalResponseTime / $TotalRequests } else { 0 }

# Get final count
$finalCount = Get-DatabaseCount

# Generate summary report
Write-Host "`n=== TEST RESULTS SUMMARY ===" -ForegroundColor Cyan
Write-Host "Total Duration: $([Math]::Round($totalDuration, 2)) seconds" -ForegroundColor White
Write-Host "Total Requests: $TotalRequests" -ForegroundColor White
Write-Host "Successful Requests: $successCount" -ForegroundColor Green
Write-Host "Failed Requests: $failureCount" -ForegroundColor Red
Write-Host "Success Rate: $([Math]::Round(($successCount / $TotalRequests) * 100, 2))%" -ForegroundColor $(if ($successCount -eq $TotalRequests) { "Green" } else { "Yellow" })
Write-Host "Average Response Time: $([Math]::Round($averageResponseTime, 2)) ms" -ForegroundColor White
Write-Host "Requests per Second: $([Math]::Round($TotalRequests / $totalDuration, 2))" -ForegroundColor White
Write-Host "Database Count Change: $($finalCount - $initialCount)" -ForegroundColor $(if (($finalCount - $initialCount) -eq $TotalRequests) { "Green" } else { "Yellow" })

# Detailed failure analysis
if ($failureCount -gt 0) {
    Write-Host "`n=== FAILURE ANALYSIS ===" -ForegroundColor Red
    $failedRequests = $results | Where-Object { -not $_.Success }
    
    # Group failures by error type
    $errorGroups = $failedRequests | Group-Object -Property { $_.Error -replace '\d+', '[NUMBER]' }
    
    foreach ($group in $errorGroups) {
        Write-Host "Error Pattern: $($group.Name)" -ForegroundColor Red
        Write-Host "Count: $($group.Count)" -ForegroundColor Red
        Write-Host "Sample Failed Data:" -ForegroundColor Red
        $sample = $group.Group | Select-Object -First 1
        Write-Host "  Name: $($sample.Data.Name) $($sample.Data.FamilyName)" -ForegroundColor Red
        Write-Host "  Email: $($sample.Data.EmailAddress)" -ForegroundColor Red
        Write-Host "  Country: $($sample.Data.CountryOfOrigin)" -ForegroundColor Red
        Write-Host ""
    }
}

# Performance analysis
Write-Host "`n=== PERFORMANCE ANALYSIS ===" -ForegroundColor Cyan
$successfulResults = $results | Where-Object { $_.Success }
if ($successfulResults.Count -gt 0) {
    $responseTimes = $successfulResults | Select-Object -ExpandProperty ResponseTime
    $minResponseTime = ($responseTimes | Measure-Object -Minimum).Minimum
    $maxResponseTime = ($responseTimes | Measure-Object -Maximum).Maximum
    
    Write-Host "Min Response Time: $([Math]::Round($minResponseTime, 2)) ms" -ForegroundColor Green
    Write-Host "Max Response Time: $([Math]::Round($maxResponseTime, 2)) ms" -ForegroundColor Green
    Write-Host "Average Response Time: $([Math]::Round($averageResponseTime, 2)) ms" -ForegroundColor Green
}

# Export detailed results to JSON for further analysis
$resultsFile = "simple-test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$results | ConvertTo-Json -Depth 10 | Out-File -FilePath $resultsFile -Encoding UTF8

Write-Host "`nDetailed results exported to: $resultsFile" -ForegroundColor Cyan

# Return summary object for further processing
return @{
    TotalRequests = $TotalRequests
    SuccessCount = $successCount
    FailureCount = $failureCount
    SuccessRate = [Math]::Round(($successCount / $TotalRequests) * 100, 2)
    AverageResponseTime = [Math]::Round($averageResponseTime, 2)
    TotalDuration = [Math]::Round($totalDuration, 2)
    RequestsPerSecond = [Math]::Round($TotalRequests / $totalDuration, 2)
    DatabaseIncrease = $finalCount - $initialCount
    ResultsFile = $resultsFile
}