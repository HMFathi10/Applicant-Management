# Test phone validation fix
Write-Host "Testing phone validation fix..." -ForegroundColor Yellow

# Test the phone cleaning logic
$testPhones = @(
    "+201001234567",
    "+20 100 123 4567", 
    "01001234567",
    "1001234567"
)

foreach ($phone in $testPhones) {
    $cleaned = $phone -replace '[\+\s]', ''
    Write-Host "Original: $phone -> Cleaned: $cleaned" -ForegroundColor Green
}

Write-Host "`nPhone validation test complete!" -ForegroundColor Yellow