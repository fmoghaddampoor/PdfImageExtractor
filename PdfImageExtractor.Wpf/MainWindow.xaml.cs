using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;

namespace PdfImageExtractor.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Process? _serverProcess;

    public MainWindow()
    {
        InitializeComponent();
        try {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (!File.Exists(iconPath)) {
                iconPath = "app.ico"; // Fallback for relative
            }

            if (File.Exists(iconPath)) {
                var icon = new System.Drawing.Icon(iconPath);
                TrayIcon.Icon = icon;
                
                // Set window icon
                this.Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            } else {
                TrayIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        } catch {
            TrayIcon.Icon = System.Drawing.SystemIcons.Application;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            this.DragMove();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PortTextBox.Text, out int port))
        {
            MessageBox.Show("Please enter a valid port number.", "Invalid Port", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            // Find the blazor project path
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Assuming we are in bin\Debug\netX.X or similar
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "PdfImageExtractor.Blazor"));
            
            if (!Directory.Exists(projectDir))
            {
                // Try alternate if published
                projectDir = Path.GetFullPath(Path.Combine(baseDir, "PdfImageExtractor.Blazor"));
            }

            string arguments;
            if (File.Exists(Path.Combine(projectDir, "PdfImageExtractor.Blazor.csproj")))
            {
                // Development mode
                arguments = $"run --project \"{projectDir}\" --urls \"http://localhost:{port}\"";
            }
            else
            {
                // Published mode
                string dllPath = Path.Combine(projectDir, "PdfImageExtractor.Blazor.dll");
                if (!File.Exists(dllPath))
                {
                    throw new Exception($"Could not find Blazor service at: {projectDir}");
                }
                arguments = $"\"{dllPath}\" --urls \"http://localhost:{port}\"";
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

            StatusTextBlock.Text = "Service Online";
            StatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(129, 199, 132)); // Light Green
            StatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(129, 199, 132)); // Light Green
            // StatusCircle updated removed for consistent icon
            
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            OpenAppButton.IsEnabled = true;
            TrayOpenApp.IsEnabled = true;
            PortTextBox.IsEnabled = false;

            // TrayIcon.ShowBalloonTip("Server Started", $"PDF Image Extractor is running on port {port}", BalloonIcon.Info);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        KillServer();
    }

    private void KillServer()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try
            {
                // Kill the process tree since 'dotnet run' starts a child process
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
        StatusTextBlock.Text = "Service Offline";
        StatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)); // Grey
        StatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)); // Grey

        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        OpenAppButton.IsEnabled = false;
        TrayOpenApp.IsEnabled = false;
        PortTextBox.IsEnabled = true;
    }

    private void OpenAppButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(PortTextBox.Text, out int port))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"http://localhost:{port}",
                UseShellExecute = true
            });
        }
    }

    private void Show_Click(object sender, RoutedEventArgs e)
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        KillServer();
        Application.Current.Shutdown();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            this.Hide();
        }
        base.OnStateChanged(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Instead of closing, hide to tray
        e.Cancel = true;
        this.Hide();
        // TrayIcon.ShowBalloonTip("Hidden", "App is still running in the tray.", BalloonIcon.Info);
    }
}