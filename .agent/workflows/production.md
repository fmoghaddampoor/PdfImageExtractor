---
description: Build all apps and run installer
---

# Production Build Workflow

This workflow builds all applications (Blazor, WPF, Installer Launcher), organizes them in the publish folder, and runs the installer.

## Steps

// turbo-all

1. Kill any running instances
```powershell
taskkill /F /IM PdfImageExtractor.Wpf.exe /T 2>$null
taskkill /F /IM dotnet.exe /FI "WINDOWTITLE eq PdfImageExtractor*" /T 2>$null
```

2. Clean and create publish folder structure
```powershell
if (Test-Path publish) { Remove-Item -Recurse -Force publish }
New-Item -ItemType Directory -Path publish/Data/Tray
New-Item -ItemType Directory -Path publish/Data/Server
```

3. Build and publish Blazor Server app
```powershell
dotnet publish PdfImageExtractor.Blazor -c Release -o publish/Data/Server
```

4. Build and publish WPF Tray app
```powershell
dotnet publish PdfImageExtractor.Wpf -c Release -o publish/Data/Tray
```

5. Build and publish Setup.exe (Installer Launcher)
```powershell
dotnet publish InstallerLauncher -c Release -r win-x64 --self-contained false -o publish /p:AssemblyName=Setup
```

6. Copy installer script to Data folder
```powershell
Copy-Item install.ps1 publish/Data/install.ps1 -Force
```

7. Run the installer
```powershell
Start-Process "publish\Setup.exe"
```
