# Test script to verify the applicant update functionality fix

Write-Host "Testing Applicant Update Functionality Fix" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Test 1: Create a test applicant
Write-Host "`nTest 1: Creating test applicant..." -ForegroundColor Yellow
$createBody = @{
    Name = "John"
    FamilyName = "Doe"
    EmailAddress = "john.doe@example.com"
    Phone = "+201234567890"
    Age = 25
    Address = "123 Main St, Cairo"
    CountryOfOrigin = "Egypt"
    AppliedDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    Hired = $false
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5259/api/applicants" -Method Post -Body $createBody -ContentType "application/json"
    $newApplicantId = $createResponse
    Write-Host "✅ Created applicant with ID: $newApplicantId" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create applicant: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Update the applicant with valid data
Write-Host "`nTest 2: Updating applicant with valid data..." -ForegroundColor Yellow
$updateBody = @{
    Id = $newApplicantId
    Name = "Johnny"
    FamilyName = "Doe-Smith"
    EmailAddress = "johnny.doe@example.com"
    Phone = "+201234567890"
    Age = 26
    Address = "456 New St, Cairo"
    CountryOfOrigin = "Egypt"
    AppliedDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    Hired = $false
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "http://localhost:5259/api/applicants/$newApplicantId" -Method Put -Body $updateBody -ContentType "application/json"
    Write-Host "✅ Successfully updated applicant" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to update applicant: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

# Test 3: Update with invalid data (should fail validation)
Write-Host "`nTest 3: Updating with invalid data (should fail validation)..." -ForegroundColor Yellow
$invalidUpdateBody = @{
    Id = $newApplicantId
    Name = "Jo"  # Too short - needs 2+ chars
    FamilyName = "Do"  # Too short - needs 2+ chars
    EmailAddress = "invalid-email"  # Invalid format
    Phone = "123"  # Too short - needs 5+ chars
    Age = 15  # Too young - needs 20-60
    Address = "Short"  # Too short - needs 10+ chars
    CountryOfOrigin = "Egypt"
    AppliedDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    Hired = $false
} | ConvertTo-Json

try {
    $invalidResponse = Invoke-RestMethod -Uri "http://localhost:5259/api/applicants/$newApplicantId" -Method Put -Body $invalidUpdateBody -ContentType "application/json"
    Write-Host "❌ Validation should have failed but didn't!" -ForegroundColor Red
} catch {
    Write-Host "✅ Validation correctly failed: $($_.Exception.Message)" -ForegroundColor Green
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Validation Response: $responseBody" -ForegroundColor Yellow
    }
}

# Test 4: Verify the applicant data integrity
Write-Host "`nTest 4: Verifying applicant data integrity..." -ForegroundColor Yellow
try {
    $verifyResponse = Invoke-RestMethod -Uri "http://localhost:5259/api/applicants/$newApplicantId" -Method Get
    Write-Host "✅ Applicant data retrieved successfully:" -ForegroundColor Green
    Write-Host "   Name: $($verifyResponse.name)" -ForegroundColor Cyan
    Write-Host "   Family Name: $($verifyResponse.familyName)" -ForegroundColor Cyan
    Write-Host "   Email: $($verifyResponse.emailAddress)" -ForegroundColor Cyan
    Write-Host "   Age: $($verifyResponse.age)" -ForegroundColor Cyan
    Write-Host "   Address: $($verifyResponse.address)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to verify applicant data: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Clean up - delete the test applicant
Write-Host "`nTest 5: Cleaning up test data..." -ForegroundColor Yellow
try {
    $deleteResponse = Invoke-RestMethod -Uri "http://localhost:5259/api/applicants/$newApplicantId" -Method Delete
    Write-Host "✅ Test applicant deleted successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to delete test applicant: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=========================================" -ForegroundColor Green
Write-Host "Test completed!" -ForegroundColor Green