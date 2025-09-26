# Applicant Management System API Testing Script
# This script demonstrates the standardized cURL pattern for all CRUD operations

$baseUrl = "http://localhost:5259/api/Applicants"
$headers = @{
    "accept" = "text/plain"
    "Content-Type" = "application/json"
}

Write-Host "=== Applicant Management System API Testing ===" -ForegroundColor Cyan
Write-Host ""

# Function to execute cURL commands
function Invoke-CurlCommand {
    param(
        [string]$method,
        [string]$url,
        [string]$data = $null
    )
    
    Write-Host "Request: $method $url" -ForegroundColor Yellow
    
    if ($data) {
        Write-Host "Body: $data" -ForegroundColor Gray
        curl -X $method -H "accept: text/plain" -H "Content-Type: application/json" -d $data $url
    } else {
        curl -X $method -H "accept: text/plain" $url
    }
    
    Write-Host ""
}

# 1. CREATE Operation - Create a new applicant
Write-Host "1. CREATE Operation" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green

$createData = @'{
  "name": "Johne",
  "familyName": "Doeeee",
  "emailAddress": "john@example.com",
  "phone": "+201234567890",
  "department": "IT",
  "countryOfOrigin": "Egypt",
  "age": 60,
  "appliedDate": "2025-09-25T01:48:38.483Z",
  "hired": true
}'@

Invoke-CurlCommand -method "POST" -url "$baseUrl" -data $createData

# 2. READ All Applicants
Write-Host "2. READ All Applicants" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green

Invoke-CurlCommand -method "GET" -url "$baseUrl"

# 3. READ Applicant by ID (Note: This endpoint is not fully implemented)
Write-Host "3. READ Applicant by ID" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green

Invoke-CurlCommand -method "GET" -url "$baseUrl/1"

# 4. UPDATE Operation (Note: This endpoint is not fully implemented)
Write-Host "4. UPDATE Operation" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green

$updateData = @'{
  "name": "John",
  "familyName": "Smith",
  "emailAddress": "john.smith@example.com",
  "phone": "+201234567890",
  "department": "HR",
  "countryOfOrigin": "Egypt",
  "age": 61,
  "appliedDate": "2025-09-25T01:48:38.483Z",
  "hired": false
}'@

Invoke-CurlCommand -method "PUT" -url "$baseUrl/1" -data $updateData

# 5. DELETE Operation (Note: This endpoint is not fully implemented)
Write-Host "5. DELETE Operation" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green

Invoke-CurlCommand -method "DELETE" -url "$baseUrl/1"

# 6. SEARCH Operation (Note: This endpoint has model binding issues)
Write-Host "6. SEARCH Operation" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green

Invoke-CurlCommand -method "GET" -url "$baseUrl/search?query=John"

# 7. FILTER Operation (Note: This endpoint has model binding issues)
Write-Host "7. FILTER Operation" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green

Invoke-CurlCommand -method "GET" -url "$baseUrl/filter?experienceMin=20&experienceMax=30"

Write-Host "=== Testing Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: Some operations are not fully implemented or have issues:" -ForegroundColor Yellow
Write-Host "- UPDATE: Not implemented (returns NoContent without actual update)" -ForegroundColor Yellow
Write-Host "- DELETE: Not implemented (returns NoContent without actual deletion)" -ForegroundColor Yellow
Write-Host "- SEARCH: Fails with 500 Internal Server Error (model binding issues)" -ForegroundColor Yellow
Write-Host "- FILTER: Fails with 500 Internal Server Error (model binding issues)" -ForegroundColor Yellow
Write-Host "- GET by ID: Returns empty results (not fully implemented)" -ForegroundColor Yellow

# Additional test examples with different data
Write-Host ""
Write-Host "=== Additional Test Examples ===" -ForegroundColor Cyan
Write-Host ""

# Create another applicant
Write-Host "Creating additional test applicant..." -ForegroundColor Blue
$additionalData = @'{
  "name": "Alice",
  "familyName": "Johnson",
  "emailAddress": "alice.johnson@example.com",
  "phone": "+201234567891",
  "department": "Marketing",
  "countryOfOrigin": "Egypt",
  "age": 45,
  "appliedDate": "2025-01-15T10:30:00.000Z",
  "hired": false
}'@

Invoke-CurlCommand -method "POST" -url "$baseUrl" -data $additionalData

# Test with invalid data to demonstrate error handling
Write-Host "Testing with invalid phone format..." -ForegroundColor Red
$invalidData = @'{
  "name": "Bob",
  "familyName": "Smith",
  "emailAddress": "bob.smith@example.com",
  "phone": "1234567890",
  "department": "Sales",
  "countryOfOrigin": "Egypt",
  "age": 35,
  "appliedDate": "2025-01-20T14:15:00.000Z",
  "hired": true
}'@

Invoke-CurlCommand -method "POST" -url "$baseUrl" -data $invalidData

Write-Host "Script execution complete!" -ForegroundColor Green