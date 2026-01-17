using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PdfImageExtractor.Wpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private Process? _serverProcess;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartServiceCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopServiceCommand))]
        [NotifyCanExecuteChangedFor(nameof(OpenAppCommand))]
        private bool _isServiceRunning;

        [ObservableProperty]
        private string _port = "5005";

        [ObservableProperty]
        private string _statusText = "Service Offline";

        [ObservableProperty]
        private Brush _statusColor = new SolidColorBrush(Color.FromRgb(136, 136, 136)); // Grey

        public MainWindowViewModel()
        {
        }

        [RelayCommand(CanExecute = nameof(CanStartService))]
        private void StartService()
        {
            if (!int.TryParse(Port, out int portNumber))
            {
                MessageBox.Show("Please enter a valid port number.", "Invalid Port", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "PdfImageExtractor.Blazor"));
                
                if (!Directory.Exists(projectDir))
                {
                    // Production Structure 1: Nested in subdirectory (Legacy)
                    projectDir = Path.GetFullPath(Path.Combine(baseDir, "PdfImageExtractor.Blazor"));
                }
                
                if (!Directory.Exists(projectDir))
                {
                    // Production Structure 2: Sibling 'Server' directory (New)
                    projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "Server"));
                }

                string arguments;
                if (File.Exists(Path.Combine(projectDir, "PdfImageExtractor.Blazor.csproj")))
                {
                    arguments = $"run --project \"{projectDir}\" --urls \"http://localhost:{portNumber}\"";
                }
                else
                {
                    string dllPath = Path.Combine(projectDir, "PdfImageExtractor.Blazor.dll");
                    if (!File.Exists(dllPath))
                    {
                        throw new Exception($"Could not find Blazor service at: {projectDir}");
                    }
                    arguments = $"\"{dllPath}\" --urls \"http://localhost:{portNumber}\"";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = projectDir
                };

                _serverProcess = new Process { StartInfo = startInfo };
                _serverProcess.Start();

                IsServiceRunning = true;
                StatusText = "Service Online";
                StatusColor = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Dark Green for Light Mode compatibility
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanStartService() => !IsServiceRunning;

        [RelayCommand(CanExecute = nameof(CanStopService))]
        private void StopService()
        {
            KillServer();
        }

        private bool CanStopService() => IsServiceRunning;

        [RelayCommand(CanExecute = nameof(CanStopService))]
        private void OpenApp()
        {
            if (int.TryParse(Port, out int port))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"http://localhost:{port}",
                    UseShellExecute = true
                });
            }
        }

        [RelayCommand]
        private void Exit()
        {
            KillServer();
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private void ShowWindow(Window window)
        {
            if (window != null)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        private void KillServer()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/F /T /PID {_serverProcess.Id}",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    })?.WaitForExit();
                }
                catch { }
            }

            _serverProcess = null;
            IsServiceRunning = false;
            StatusText = "Service Offline";
            StatusColor = new SolidColorBrush(Color.FromRgb(136, 136, 136)); // Grey
        }
    }
}
