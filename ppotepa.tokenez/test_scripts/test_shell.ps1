# PowerScript Interactive Shell Test Script
# This script demonstrates the enhanced shell capabilities

Write-Host "Testing PowerScript Interactive Shell..." -ForegroundColor Cyan
Write-Host ""

# Test 1: Show help
Write-Host "Test 1: Displaying help..." -ForegroundColor Yellow
Write-Host "Commands that will be sent to shell:"
Write-Host "  HELP" -ForegroundColor Green
Write-Host "  VERSION" -ForegroundColor Green
Write-Host "  ABOUT" -ForegroundColor Green
Write-Host ""

# Test 2: Execute some PowerScript code
Write-Host "Test 2: PowerScript execution examples..." -ForegroundColor Yellow
Write-Host "Example commands to try interactively:"
Write-Host "  PRINT `"Hello from PowerScript!`"" -ForegroundColor Green
Write-Host "  NET::System.Console.WriteLine(`"Hello from .NET!`")" -ForegroundColor Green
Write-Host "  EXECUTE `"helper.ps`"" -ForegroundColor Green
Write-Host ""

# Test 3: Multi-line function
Write-Host "Test 3: Multi-line function example..." -ForegroundColor Yellow
Write-Host "Try entering this function (note the >> prompt for continuation):" -ForegroundColor Green
Write-Host "  FUNCTION GREET(STRING name) {" -ForegroundColor White
Write-Host "    PRINT name" -ForegroundColor White
Write-Host "  }" -ForegroundColor White
Write-Host ""

Write-Host "Now starting the interactive shell..." -ForegroundColor Cyan
Write-Host "(Type 'EXIT' to quit, 'HELP' for commands)" -ForegroundColor DarkGray
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
