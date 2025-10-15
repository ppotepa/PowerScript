# PowerScript Rename Script
# This script renames all Tokenez references to PowerScript

param(
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== PowerScript Rename Script ===" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

$rootPath = "D:\git\ppotepa\tokenez\tokenez"
Set-Location $rootPath

# Step 1: Clean build artifacts first
Write-Host "Step 1: Cleaning build artifacts..." -ForegroundColor Green
$objDirs = Get-ChildItem -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue
$binDirs = Get-ChildItem -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue

Write-Host "  Found $($objDirs.Count) obj directories and $($binDirs.Count) bin directories"

if (-not $DryRun) {
    $objDirs | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    $binDirs | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Cleaned!" -ForegroundColor Green
}
Write-Host ""

# Step 2: Update file contents (before renaming files/folders)
Write-Host "Step 2: Updating file contents..." -ForegroundColor Green

# Define replacements
$replacements = @(
    @{Pattern = 'namespace Tokenez\.Core'; Replacement = 'namespace PowerScript.Core'},
    @{Pattern = 'namespace Tokenez\.Common'; Replacement = 'namespace PowerScript.Common'},
    @{Pattern = 'namespace Tokenez\.Parser'; Replacement = 'namespace PowerScript.Parser'},
    @{Pattern = 'namespace Tokenez\.Compiler'; Replacement = 'namespace PowerScript.Compiler'},
    @{Pattern = 'namespace Tokenez\.Runtime'; Replacement = 'namespace PowerScript.Runtime'},
    @{Pattern = 'namespace Tokenez\.Interpreter'; Replacement = 'namespace PowerScript.Interpreter'},
    @{Pattern = 'namespace Tokenez\.CLI'; Replacement = 'namespace PowerScript.CLI'},
    @{Pattern = 'namespace Tokenez\.Integration\.Tests'; Replacement = 'namespace PowerScript.Integration.Tests'},
    
    @{Pattern = 'using Tokenez\.Core'; Replacement = 'using PowerScript.Core'},
    @{Pattern = 'using Tokenez\.Common'; Replacement = 'using PowerScript.Common'},
    @{Pattern = 'using Tokenez\.Parser'; Replacement = 'using PowerScript.Parser'},
    @{Pattern = 'using Tokenez\.Compiler'; Replacement = 'using PowerScript.Compiler'},
    @{Pattern = 'using Tokenez\.Runtime'; Replacement = 'using PowerScript.Runtime'},
    @{Pattern = 'using Tokenez\.Interpreter'; Replacement = 'using PowerScript.Interpreter'},
    
    @{Pattern = '<RootNamespace>Tokenez\.Core</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Core</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.Common</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Common</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.Parser</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Parser</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.Compiler</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Compiler</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.Runtime</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Runtime</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.Interpreter</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Interpreter</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.CLI</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.CLI</RootNamespace>'},
    @{Pattern = '<RootNamespace>Tokenez\.Integration\.Tests</RootNamespace>'; Replacement = '<RootNamespace>PowerScript.Integration.Tests</RootNamespace>'},
    
    @{Pattern = 'Tokenez\.Runtime\.csproj'; Replacement = 'PowerScript.Runtime.csproj'},
    @{Pattern = 'Tokenez\.Integration\.Tests\.csproj'; Replacement = 'PowerScript.Integration.Tests.csproj'},
    
    @{Pattern = 'src\\Tokenez\.Core\\'; Replacement = 'src\PowerScript.Core\'},
    @{Pattern = 'src\\Tokenez\.Common\\'; Replacement = 'src\PowerScript.Common\'},
    @{Pattern = 'src\\Tokenez\.Parser\\'; Replacement = 'src\PowerScript.Parser\'},
    @{Pattern = 'src\\Tokenez\.Compiler\\'; Replacement = 'src\PowerScript.Compiler\'},
    @{Pattern = 'src\\Tokenez\.Runtime\\'; Replacement = 'src\PowerScript.Runtime\'},
    @{Pattern = 'src\\Tokenez\.Interpreter\\'; Replacement = 'src\PowerScript.Interpreter\'},
    @{Pattern = 'src\\Tokenez\.CLI\\'; Replacement = 'src\PowerScript.CLI\'},
    @{Pattern = 'tests\\Tokenez\.Integration\.Tests\\'; Replacement = 'tests\PowerScript.Integration.Tests\'},
    
    @{Pattern = 'ppotepa\.tokenez\.exe'; Replacement = 'powerscript.exe'},
    @{Pattern = 'tokenez\.exe'; Replacement = 'powerscript.exe'},
    @{Pattern = 'Usage: tokenez'; Replacement = 'Usage: powerscript'},
    @{Pattern = 'Compile from file: tokenez'; Replacement = 'Compile from file: powerscript'},
    
    @{Pattern = 'ppotepa\.tokenez'; Replacement = 'PowerScript'},
    @{Pattern = 'd:\\git\\ppotepa\\tokenez\\tokenez'; Replacement = 'd:\git\ppotepa\powerscript\powerscript'},
    @{Pattern = 'ppotepa/tokenez'; Replacement = 'ppotepa/powerscript'}
)

# Get all relevant files
$files = Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.sln,*.slnx,*.md -Exclude RENAME_CHECKLIST.md,rename_to_powerscript.ps1

Write-Host "  Processing $($files.Count) files..."
$updatedCount = 0

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Stop
        $originalContent = $content
        $fileUpdated = $false
        
        foreach ($replacement in $replacements) {
            if ($content -match $replacement.Pattern) {
                $content = $content -replace $replacement.Pattern, $replacement.Replacement
                $fileUpdated = $true
            }
        }
        
        if ($fileUpdated) {
            $updatedCount++
            if ($Verbose) {
                Write-Host "    Updated: $($file.FullName.Replace($rootPath, '.'))" -ForegroundColor Yellow
            }
            
            if (-not $DryRun) {
                Set-Content -Path $file.FullName -Value $content -NoNewline
            }
        }
    }
    catch {
        Write-Host "    Error processing $($file.FullName): $_" -ForegroundColor Red
    }
}

Write-Host "  Updated $updatedCount files" -ForegroundColor Green
Write-Host ""

# Step 3: Rename project files
Write-Host "Step 3: Renaming project files..." -ForegroundColor Green

$fileRenames = @(
    @{Old = "src\Tokenez.Runtime\Tokenez.Runtime.csproj"; New = "src\Tokenez.Runtime\PowerScript.Runtime.csproj"},
    @{Old = "tests\Tokenez.Integration.Tests\Tokenez.Integration.Tests.csproj"; New = "tests\Tokenez.Integration.Tests\PowerScript.Integration.Tests.csproj"},
    @{Old = "PoweScript.sln"; New = "PowerScript.sln"},
    @{Old = "ppotepa.tokenez.slnx"; New = "ppotepa.powerscript.slnx"}
)

foreach ($rename in $fileRenames) {
    $oldPath = Join-Path $rootPath $rename.Old
    $newPath = Join-Path $rootPath $rename.New
    
    if (Test-Path $oldPath) {
        Write-Host "  Renaming: $($rename.Old) -> $($rename.New)" -ForegroundColor Yellow
        
        if (-not $DryRun) {
            Move-Item -Path $oldPath -Destination $newPath -Force
        }
    }
    else {
        Write-Host "  Skipping (not found): $($rename.Old)" -ForegroundColor Gray
    }
}
Write-Host ""

# Step 4: Rename directories (do this last, from deepest to shallowest)
Write-Host "Step 4: Renaming directories..." -ForegroundColor Green

$dirRenames = @(
    @{Old = "src\Tokenez.Core"; New = "src\PowerScript.Core"},
    @{Old = "src\Tokenez.Common"; New = "src\PowerScript.Common"},
    @{Old = "src\Tokenez.Parser"; New = "src\PowerScript.Parser"},
    @{Old = "src\Tokenez.Compiler"; New = "src\PowerScript.Compiler"},
    @{Old = "src\Tokenez.Runtime"; New = "src\PowerScript.Runtime"},
    @{Old = "src\Tokenez.Interpreter"; New = "src\PowerScript.Interpreter"},
    @{Old = "src\Tokenez.CLI"; New = "src\PowerScript.CLI"},
    @{Old = "tests\Tokenez.Integration.Tests"; New = "tests\PowerScript.Integration.Tests"}
)

foreach ($rename in $dirRenames) {
    $oldPath = Join-Path $rootPath $rename.Old
    $newPath = Join-Path $rootPath $rename.New
    
    if (Test-Path $oldPath) {
        Write-Host "  Renaming directory: $($rename.Old) -> $($rename.New)" -ForegroundColor Yellow
        
        if (-not $DryRun) {
            Move-Item -Path $oldPath -Destination $newPath -Force
        }
    }
    else {
        Write-Host "  Skipping (not found): $($rename.Old)" -ForegroundColor Gray
    }
}
Write-Host ""

# Step 5: Summary
Write-Host "=== Rename Complete ===" -ForegroundColor Cyan
Write-Host ""
if ($DryRun) {
    Write-Host "This was a DRY RUN - no changes were made" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply changes" -ForegroundColor Yellow
}
else {
    Write-Host "Next steps:" -ForegroundColor Green
    Write-Host "  1. Reopen your IDE/editor"
    Write-Host "  2. Run: dotnet restore"
    Write-Host "  3. Run: dotnet build"
    Write-Host "  4. Run: dotnet test"
    Write-Host "  5. Commit changes to git"
    Write-Host ""
    Write-Host "Optional: Rename parent directory from 'tokenez' to 'powerscript'" -ForegroundColor Yellow
}
Write-Host ""
