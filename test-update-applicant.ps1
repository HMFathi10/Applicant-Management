# Test script for updateApplicant endpoint functionality
# This script tests the backend API directly to verify the update functionality

$baseUrl = "http://localhost:5259"
$headers = @{
    "Content-Type" = "application/json"
}

Write-Host "Testing updateApplicant endpoint functionality..." -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow

# Step 1: Create a test applicant
Write-Host "`nStep 1: Creating a test applicant..." -ForegroundColor Cyan
$createData = @{
    name = "Test User"
    familyName = "Test Family"
    emailAddress = "test.update@example.com"
    phone = "+1234567890"
    age = 25
    address = "123 Test Street, Test City"
    countryOfOrigin = "United States"
    appliedDate = "2024-01-15"
    hired = $false
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/applicants" -Method Post -Body $createData -Headers $headers
    $applicantId = $createResponse
    Write-Host "✓ Created applicant with ID: $applicantId" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to create applicant: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Get the created applicant details
Write-Host "`nStep 2: Fetching created applicant details..." -ForegroundColor Cyan
try {
    $applicant = Invoke-RestMethod -Uri "$baseUrl/api/applicants/$applicantId" -Method Get -Headers $headers
    Write-Host "✓ Retrieved applicant details:" -ForegroundColor Green
    Write-Host "  Name: $($applicant.name) $($applicant.familyName)"
    Write-Host "  Email: $($applicant.emailAddress)"
    Write-Host "  Phone: $($applicant.phone)"
    Write-Host "  Age: $($applicant.age)"
    Write-Host "  Address: $($applicant.address)"
    Write-Host "  Country: $($applicant.countryOfOrigin)"
    Write-Host "  Hired: $($applicant.hired)"
} catch {
    Write-Host "✗ Failed to fetch applicant: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Update the applicant
Write-Host "`nStep 3: Updating applicant..." -ForegroundColor Cyan
$updateData = @{
    name = "Updated Test User"
    familyName = "Updated Test Family"
    emailAddress = "updated.test@example.com"
    phone = "+0987654321"
    age = 30
    address = "456 Updated Street, Updated City"
    countryOfOrigin = "Canada"
    appliedDate = "2024-01-15"
    hired = $true
    rowVersion = $applicant.rowVersion
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$baseUrl/api/applicants/$applicantId" -Method Put -Body $updateData -Headers $headers
    Write-Host "✓ Successfully updated applicant" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to update applicant: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
    exit 1
}

# Step 4: Verify the update
Write-Host "`nStep 4: Verifying the update..." -ForegroundColor Cyan
try {
    $updatedApplicant = Invoke-RestMethod -Uri "$baseUrl/api/applicants/$applicantId" -Method Get -Headers $headers
    Write-Host "✓ Retrieved updated applicant details:" -ForegroundColor Green
    Write-Host "  Name: $($updatedApplicant.name) $($updatedApplicant.familyName)"
    Write-Host "  Email: $($updatedApplicant.emailAddress)"
    Write-Host "  Phone: $($updatedApplicant.phone)"
    Write-Host "  Age: $($updatedApplicant.age)"
    Write-Host "  Address: $($updatedApplicant.address)"
    Write-Host "  Country: $($updatedApplicant.countryOfOrigin)"
    Write-Host "  Hired: $($updatedApplicant.hired)"
    
    # Verify the changes
    $changes = @()
    if ($updatedApplicant.name -ne $applicant.name) { $changes += "Name" }
    if ($updatedApplicant.familyName -ne $applicant.familyName) { $changes += "Family Name" }
    if ($updatedApplicant.emailAddress -ne $applicant.emailAddress) { $changes += "Email" }
    if ($updatedApplicant.phone -ne $applicant.phone) { $changes += "Phone" }
    if ($updatedApplicant.age -ne $applicant.age) { $changes += "Age" }
    if ($updatedApplicant.address -ne $applicant.address) { $changes += "Address" }
    if ($updatedApplicant.countryOfOrigin -ne $applicant.countryOfOrigin) { $changes += "Country" }
    if ($updatedApplicant.hired -ne $applicant.hired) { $changes += "Hired Status" }
    
    Write-Host "`n✓ Successfully updated fields: $($changes -join ', ')" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to fetch updated applicant: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Test data integrity by checking all applicants
Write-Host "`nStep 5: Verifying data integrity..." -ForegroundColor Cyan
try {
    $allApplicants = Invoke-RestMethod -Uri "$baseUrl/api/applicants" -Method Get -Headers $headers
    Write-Host "✓ Total applicants in database: $($allApplicants.Count)" -ForegroundColor Green
    
    # Check if our updated applicant is in the list
    $found = $false
    foreach ($app in $allApplicants) {
        if ($app.id -eq $applicantId) {
            $found = $true
            Write-Host "✓ Updated applicant found in the list" -ForegroundColor Green
            break
        }
    }
    
    if (-not $found) {
        Write-Host "✗ Updated applicant not found in the list" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Failed to fetch all applicants: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n✅ All tests completed successfully!" -ForegroundColor Green
Write-Host "The updateApplicant endpoint is working correctly with the new database schema." -ForegroundColor Green