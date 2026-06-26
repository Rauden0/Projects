# RssProxy Dependency Checker
# Run this script to verify all required tools are available

Write-Host "`n=== RssProxy Dependency Checker ===" -ForegroundColor Cyan
Write-Host ""

$allOk = $true

# Check CMake
Write-Host "[1/4] Checking CMake..." -NoNewline
$cmake = Get-Command cmake -ErrorAction SilentlyContinue
if ($cmake) {
    $version = (cmake --version 2>&1 | Select-Object -First 1) -replace 'cmake version ', ''
    Write-Host " ✓ Found ($version)" -ForegroundColor Green
} else {
    Write-Host " ✗ Not found in PATH" -ForegroundColor Red
    Write-Host "      Run: winget install --id Kitware.CMake" -ForegroundColor Yellow
    $allOk = $false
}

# Check C++ Compiler
Write-Host "[2/4] Checking C++ Compiler..." -NoNewline
$cl = Get-Command cl -ErrorAction SilentlyContinue
if ($cl) {
    Write-Host " ✓ MSVC Found" -ForegroundColor Green
} else {
    # Try to find via vswhere
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath 2>$null
        if ($vsPath) {
            Write-Host " ✓ Visual Studio Build Tools found" -ForegroundColor Green
            Write-Host "      Note: Run from 'Developer Command Prompt' or 'Developer PowerShell'" -ForegroundColor Yellow
        } else {
            Write-Host " ✗ Not found" -ForegroundColor Red
            Write-Host "      Install Visual Studio Build Tools with C++ workload" -ForegroundColor Yellow
            $allOk = $false
        }
    } else {
        Write-Host " ? Unable to detect" -ForegroundColor Yellow
        Write-Host "      Install Visual Studio Build Tools with C++ workload" -ForegroundColor Yellow
        $allOk = $false
    }
}

# Check Qt
Write-Host "[3/4] Checking Qt..." -NoNewline
$qtPaths = @(
    "$PSScriptRoot\Tests\6.8.0\msvc2022_64",
    "C:\Qt\6.8.0\msvc2022_64",
    "C:\Qt\6.7.0\msvc2022_64",
    "C:\Qt\6.6.0\msvc2022_64",
    "C:\Qt\6.5.0\msvc2022_64",
    "C:\Qt6",
    "$env:CMAKE_PREFIX_PATH"
)

$qtFound = $false
foreach ($path in $qtPaths) {
    if ($path -and (Test-Path "$path\bin\qmake.exe" -ErrorAction SilentlyContinue)) {
        $qtVersion = & "$path\bin\qmake.exe" -query QT_VERSION 2>$null
        Write-Host " ✓ Found Qt $qtVersion at $path" -ForegroundColor Green
        $qtFound = $true
        break
    }
}

if (-not $qtFound) {
    Write-Host " ✗ Not found" -ForegroundColor Red
    Write-Host "      See SETUP.md for installation options" -ForegroundColor Yellow
    $allOk = $false
}

# Check Python (for aqt)
Write-Host "[4/4] Checking Python (optional, for aqt installer)..." -NoNewline
$python = Get-Command python -ErrorAction SilentlyContinue
if ($python) {
    $pyVersion = (python --version 2>&1) -replace 'Python ', ''
    Write-Host " ✓ Found ($pyVersion)" -ForegroundColor Green

    $aqtCheck = python -m pip show aqtinstall 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "      aqtinstall is installed" -ForegroundColor Green
    } else {
        Write-Host "      To install aqt: python -m pip install aqtinstall" -ForegroundColor Gray
    }
} else {
    Write-Host " - Not found (optional)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
if ($allOk) {
    Write-Host "✓ All dependencies are ready!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  cd Tests"
    Write-Host "  cmake -B build"
    Write-Host "  cmake --build build"
    Write-Host "  ctest --test-dir build --output-on-failure"
} else {
    Write-Host "✗ Some dependencies are missing. Please check the items marked above." -ForegroundColor Red
    Write-Host ""
    Write-Host "See SETUP.md for detailed installation instructions." -ForegroundColor Yellow
}
Write-Host ""

