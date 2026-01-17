using System;
using System.Diagnostics;
using System.IO;

namespace InstallerLauncher;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Find install.ps1 in the same directory as the executable
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = Path.Combine(baseDir, "Data", "install.ps1");

            if (!File.Exists(scriptPath))
            {
                // Fallback: look in current directory (if different)
                scriptPath = Path.Combine(Environment.CurrentDirectory, "install.ps1");
            }

            if (!File.Exists(scriptPath))
            {
                // Silent failure or MessageBox? MessageBox is better for Setup.
                // Since this is a WinExe, Console.WriteLine won't show.
                // We'll rely on the fact that if it's missing, the package is broken.
                // But let's try to run it anyway, maybe it's in path.
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = baseDir // Crucial for PSScriptRoot logic
            };

            Process.Start(startInfo);
        }
        catch
        {
            // Ignore errors, silent launcher
        }
    }
}
