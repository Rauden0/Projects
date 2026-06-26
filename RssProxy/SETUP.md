# RssProxy Setup Guide

## Prerequisites

### 1. CMake
✅ **Already configured!** CMake is installed and added to your PATH.

After restarting PowerShell, verify with:
```powershell
cmake --version
```

### 2. C++ Compiler
✅ **Found:** Visual Studio Build Tools 2022 with MSVC 19.44 is installed.

### 3. Qt Framework (Required)

The project requires Qt6 (or Qt5) with the following modules:
- Core
- Network  
- HttpServer
- Sql
- Test

#### Installation Options:

##### Option A: Qt Online Installer (Recommended)
1. Download from: https://www.qt.io/download-qt-installer
2. Run the installer
3. Select **Qt 6.8.0** (or latest 6.x)
4. Choose **MSVC 2022 64-bit** component
5. Additional libraries:
   - Qt HTTP Server
   - Qt Network Authorization

Installation size: ~3-5 GB

##### Option B: aqt-install (Command-line, Lightweight)
If you have sufficient disk space:

```powershell
# Already installed: python -m pip install aqtinstall

# Install Qt 6.8.0 (requires ~4 GB free space)
python -m aqt install-qt windows desktop 6.8.0 win64_msvc2022_64 -m qthttpserver qtnetworkauth --outputdir C:\Qt
```

Then set Qt path:
```powershell
$env:CMAKE_PREFIX_PATH = "C:\Qt\6.8.0\msvc2022_64"
```

##### Option C: vcpkg (Build from source)
```powershell
# Clone vcpkg
git clone https://github.com/Microsoft/vcpkg.git C:\vcpkg
cd C:\vcpkg
.\bootstrap-vcpkg.bat

# Install Qt (will take time to build)
.\vcpkg install qt6-base qt6-httpserver qt6-networkauth:x64-windows
```

## Building the Project

### Server
```powershell
cd C:\Users\werti\Desktop\github\Projects\RssProxy\Server
cmake -B build
cmake --build build --config Release
```

### Client
```powershell
cd C:\Users\werti\Desktop\github\Projects\RssProxy\Client
cmake -B build
cmake --build build --config Release
```

### Tests (After Qt is installed)
```powershell
cd C:\Users\werti\Desktop\github\Projects\RssProxy\Tests
cmake -B build -DCMAKE_PREFIX_PATH="C:\Qt\6.8.0\msvc2022_64"  # Adjust Qt path
cmake --build build
ctest --test-dir build --output-on-failure
```

## Current Status

- ✅ CMake: Installed and configured
- ✅ MSVC Compiler: Visual Studio Build Tools 2022
- ✅ Qt Framework: bundled under `Tests/6.8.0/msvc2022_64` (includes HttpServer + WebSockets)

## Troubleshooting

### CMake not found after setup
Restart your PowerShell session or run:
```powershell
$env:Path = "C:\Program Files\CMake\bin;" + $env:Path
```

### Qt not found during cmake configure
Set the Qt installation path:
```powershell
$env:CMAKE_PREFIX_PATH = "C:\Path\To\Qt\6.x.x\msvc2022_64"
```

Or pass it directly to cmake:
```powershell
cmake -B build -DCMAKE_PREFIX_PATH="C:\Path\To\Qt\6.x.x\msvc2022_64"
```

### Disk space issues
- Qt full installation requires 3-5 GB
- Consider freeing up space or using an external drive
- Minimal Qt installation: ~2 GB with only required modules

