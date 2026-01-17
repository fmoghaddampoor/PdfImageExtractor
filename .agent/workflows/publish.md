---
description: Kill running app, publish Blazor and WPF, then run the tray app.
---

// turbo-all

1. Kill any running instances of the app
```powershell
taskkill /F /IM PdfImageExtractor.Wpf.exe /T 2>$null
taskkill /F /IM dotnet.exe /FI "WINDOWTITLE eq PdfImageExtractor*" /T 2>$null
```

2. Create publish directory
```powershell
if (Test-Path publish) { Remove-Item -Recurse -Force publish }
New-Item -ItemType Directory -Path publish
```

3. Publish Blazor Server App
```powershell
dotnet publish PdfImageExtractor.Blazor -c Release -o publish/PdfImageExtractor.Blazor
```

4. Publish WPF Tray App
```powershell
dotnet publish PdfImageExtractor.Wpf -c Release -o publish
```

5. Copy favicon to publish root (if needed by WPF icon logic)
```powershell
Copy-Item PdfImageExtractor.Blazor/wwwroot/favicon.png publish/favicon.ico -Force
```

6. Run the published Tray App
```powershell
Start-Process "publish/PdfImageExtractor.Wpf.exe"
```
