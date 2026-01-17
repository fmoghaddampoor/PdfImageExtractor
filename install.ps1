<# :
  .NET Host Hack to Allow dual-mode execution (Console/GUI) if needed.
#>

# 1. Hide Console Window Immediately
if ($Host.Name -eq "ConsoleHost") {
    $code = '[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); [DllImport("kernel32.dll")] public static extern IntPtr GetConsoleWindow();'
    $type = Add-Type -MemberDefinition $code -Name "Win32" -Namespace Win32 -PassThru
    $type::ShowWindow($type::GetConsoleWindow(), 0) # 0 = Hide
}

Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Drawing, System.Windows.Forms

# Application Constants
$AppName = "PdfImageExtractor"
$AppExe = "PdfImageExtractor.Wpf.exe"
$SourceDir = $PSScriptRoot
$DefaultTargetDir = "C:\PdfImageExtractor"
$DesktopDir = [Environment]::GetFolderPath("Desktop")
$StartMenuDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\$AppName"

# XAML Definition
[xml]$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="$AppName Setup" Height="450" Width="700"
        WindowStartupLocation="CenterScreen" ResizeMode="NoResize" WindowStyle="None"
        Background="White" AllowsTransparency="True">
    
    <Window.Resources>
        <Style x:Key="SidebarItem" TargetType="TextBlock">
            <Setter Property="Foreground" Value="#AAAAAA"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="20,10"/>
        </Style>
        <Style x:Key="SidebarItemActive" TargetType="TextBlock" BasedOn="{StaticResource SidebarItem}">
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
        </Style>
        
        <Style x:Key="PrimaryButton" TargetType="Button">
            <Setter Property="Background" Value="#4F46E5"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="Padding" Value="25,10"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}" CornerRadius="6">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center" Margin="{TemplateBinding Padding}"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
             <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#4338CA"/>
                </Trigger>
                <Trigger Property="IsEnabled" Value="False">
                    <Setter Property="Opacity" Value="0.6"/>
                </Trigger>
            </Style.Triggers>
        </Style>
        
        <Style x:Key="SecondaryButton" TargetType="Button" BasedOn="{StaticResource PrimaryButton}">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#666666"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="BorderBrush" Value="#CCCCCC"/>
             <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#F5F5F5"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <Border BorderBrush="#DDDDDD" BorderThickness="1">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="220"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <!-- Sidebar -->
            <Border Grid.Column="0" Background="#1E293B">
                <StackPanel>
                    <StackPanel Margin="20,30,20,40">
                         <!-- Simplified Logo Placeholder (White Box for now) -->
                        <Border Width="40" Height="40" Background="White" HorizontalAlignment="Left" CornerRadius="8" Margin="0,0,0,15">
                            <TextBlock Text="P" Foreground="#1E293B" FontSize="24" FontWeight="Bold" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                        <TextBlock Text="PDF Image" Foreground="White" FontSize="18" FontWeight="Bold"/>
                        <TextBlock Text="Extractor" Foreground="White" FontSize="18" FontWeight="Bold"/>
                    </StackPanel>

                    <TextBlock Name="Step1Text" Text="1. Welcome" Style="{StaticResource SidebarItemActive}"/>
                    <TextBlock Name="Step2Text" Text="2. Install Location" Style="{StaticResource SidebarItem}"/>
                    <TextBlock Name="Step3Text" Text="3. Processing" Style="{StaticResource SidebarItem}"/>
                    <TextBlock Name="Step4Text" Text="4. Finish" Style="{StaticResource SidebarItem}"/>
                </StackPanel>
            </Border>

            <!-- Main Content Area -->
            <Grid Grid.Column="1" Background="White">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/> <!-- Top Bar -->
                    <RowDefinition Height="*"/>    <!-- Content -->
                    <RowDefinition Height="80"/>   <!-- Footer -->
                </Grid.RowDefinitions>

                <!-- Top Bar -->
                <Grid Grid.Row="0" Margin="20">
                    <TextBlock Name="HeaderTitle" Text="Setup Wizard" FontSize="24" FontWeight="Light" Foreground="#333333"/>
                    <Button Name="CloseBtn" Content="✕" HorizontalAlignment="Right" VerticalAlignment="Top" 
                            Background="Transparent" Foreground="#999999" BorderThickness="0" FontSize="16" Cursor="Hand" Margin="0,-10,-10,0"/>
                </Grid>

                <Grid Grid.Row="1" Margin="30,10,30,0">
                    <!-- VIEW 1: WELCOME -->
                    <StackPanel Name="View1_Welcome">
                         <TextBlock Text="Welcome to the PDF Image Extractor Setup Wizard" TextWrapping="Wrap" FontSize="16" FontWeight="SemiBold" Foreground="#333333" Margin="0,0,0,20"/>
                         <TextBlock Text="This utility will install the PDF Image Extractor on your computer." TextWrapping="Wrap" Foreground="#555555" Margin="0,0,0,10" FontSize="14"/>
                         <TextBlock Text="Features:" TextWrapping="Wrap" Foreground="#555555" Margin="0,20,0,10" FontWeight="SemiBold"/>
                         <TextBlock Text="• Extract images from PDF files easily" Foreground="#555555" Margin="10,0,0,5"/>
                         <TextBlock Text="• High-quality batch processing" Foreground="#555555" Margin="10,0,0,5"/>
                         <TextBlock Text="• Automated system tray integration" Foreground="#555555" Margin="10,0,0,5"/>
                    </StackPanel>

                    <!-- VIEW 2: PATH -->
                    <StackPanel Name="View2_Path" Visibility="Collapsed">
                        <TextBlock Text="Select Installation Folder" FontSize="16" FontWeight="SemiBold" Foreground="#333333" Margin="0,0,0,20"/>
                        <TextBlock Text="Any existing installation at the target location will be replaced." Foreground="#FF9800" Margin="0,0,0,10" FontSize="13"/>
                        
                        <Grid Margin="0,10">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBox Name="PathTextBox" Grid.Column="0" Height="35" VerticalContentAlignment="Center" Padding="8" BorderBrush="#CCCCCC" FontSize="13"/>
                            <Button Name="BrowseBtn" Grid.Column="1" Content="..." Width="40" Height="35" Margin="5,0,0,0" Style="{StaticResource SecondaryButton}"/>
                        </Grid>
                        <TextBlock Text="Required Space: ~40 MB" Foreground="#888888" FontSize="12"/>
                    </StackPanel>

                    <!-- VIEW 3: INSTALLING -->
                    <StackPanel Name="View3_Installing" Visibility="Collapsed" VerticalAlignment="Center">
                        <TextBlock Name="StatusText" Text="Preparing..." FontSize="16" Foreground="#333333" Margin="0,0,0,15" HorizontalAlignment="Center"/>
                        <ProgressBar Name="ProgressBar" Height="8" Background="#F0F0F0" BorderThickness="0" Foreground="#4F46E5" Value="0" Maximum="100"/>
                        <TextBlock Name="StatusDetail" Text="" FontSize="12" Foreground="#888888" Margin="0,10,0,0" HorizontalAlignment="Center"/>
                    </StackPanel>

                    <!-- VIEW 4: SUCCESS -->
                    <StackPanel Name="View4_Success" Visibility="Collapsed" VerticalAlignment="Center">
                         <TextBlock Text="Installation Complete" FontSize="20" FontWeight="SemiBold" Foreground="#10B981" HorizontalAlignment="Center" Margin="0,0,0,15"/>
                         <TextBlock Text="The application has been successfully installed." Foreground="#555555" HorizontalAlignment="Center"/>
                         <Border Background="#F9FAFB" Padding="15" Margin="0,20,0,0" CornerRadius="6">
                             <TextBlock Name="FinalPathText" Text="" Foreground="#6B7280" FontSize="12" TextWrapping="Wrap" HorizontalAlignment="Center"/>
                         </Border>
                    </StackPanel>
                </Grid>

                <!-- Footer -->
                <Border Grid.Row="2" Background="#FFFFFF" Padding="30,20" BorderThickness="0,1,0,0" BorderBrush="#F0F0F0">
                    <Grid>
                        <Button Name="BackBtn" Content="Back" Style="{StaticResource SecondaryButton}" HorizontalAlignment="Left" Width="100" Visibility="Hidden"/>
                        
                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                             <Button Name="NextBtn" Content="Next" Style="{StaticResource PrimaryButton}" Width="120"/>
                             <Button Name="FinishBtn" Content="Launch App" Style="{StaticResource PrimaryButton}" Width="140" Visibility="Collapsed"/>
                        </StackPanel>
                    </Grid>
                </Border>
            </Grid>
        </Grid>
    </Border>
</Window>
"@

# Helper to pump messages safely
function DoEvents {
    [System.Windows.Threading.Dispatcher]::CurrentDispatcher.Invoke([Action] {}, [System.Windows.Threading.DispatcherPriority]::Background)
}

# --- LOGIC ---
$reader = (New-Object System.Xml.XmlNodeReader $xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

# Control References
$controls = @{
    Window        = $window
    CloseBtn      = $window.FindName("CloseBtn")
    BackBtn       = $window.FindName("BackBtn")
    NextBtn       = $window.FindName("NextBtn")
    FinishBtn     = $window.FindName("FinishBtn")
    
    PathTextBox   = $window.FindName("PathTextBox")
    BrowseBtn     = $window.FindName("BrowseBtn")
    ProgressBar   = $window.FindName("ProgressBar")
    StatusText    = $window.FindName("StatusText")
    StatusDetail  = $window.FindName("StatusDetail")
    FinalPathText = $window.FindName("FinalPathText")
    
    # Views
    View1         = $window.FindName("View1_Welcome")
    View2         = $window.FindName("View2_Path")
    View3         = $window.FindName("View3_Installing")
    View4         = $window.FindName("View4_Success")

    # Step Labels
    Step1         = $window.FindName("Step1Text")
    Step2         = $window.FindName("Step2Text")
    Step3         = $window.FindName("Step3Text")
    Step4         = $window.FindName("Step4Text")
}

# Values
$CurrentStep = 1
$controls.PathTextBox.Text = $DefaultTargetDir

# --- Styles Helpers ---
function Set-ActiveStep ($stepNum) {
    # Reset all
    1..4 | ForEach-Object { $controls["Step$_"].Opacity = 0.6; $controls["Step$_"].FontWeight = "Normal"; $controls["Step$_"].Foreground = "#AAAAAA" }
    # Set Active
    $t = $controls["Step$stepNum"]
    $t.Opacity = 1.0
    $t.FontWeight = "SemiBold"
    $t.Foreground = "White"
}

# --- Event Handlers ---

$controls.Window.Add_MouseLeftButtonDown({ $script:controls.Window.DragMove() })
$controls.CloseBtn.Add_Click({ $controls.Window.Close() })

$controls.BrowseBtn.Add_Click({
        $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
        $dialog.SelectedPath = $controls.PathTextBox.Text
        if ($dialog.ShowDialog() -eq "OK") {
            $controls.PathTextBox.Text = $dialog.SelectedPath
        }
    })

$controls.BackBtn.Add_Click({
        if ($CurrentStep -eq 2) {
            $controls.View2.Visibility = "Collapsed"
            $controls.View1.Visibility = "Visible"
            $controls.BackBtn.Visibility = "Hidden"
            Set-ActiveStep 1
            $script:CurrentStep = 1
        }
    })

$controls.NextBtn.Add_Click({
        if ($CurrentStep -eq 1) {
            $controls.View1.Visibility = "Collapsed"
            $controls.View2.Visibility = "Visible"
            $controls.BackBtn.Visibility = "Visible"
            $controls.NextBtn.Content = "Install"
            Set-ActiveStep 2
            $script:CurrentStep = 2
            return
        }

        if ($CurrentStep -eq 2) {
            # START INSTALLATION
            $target = $controls.PathTextBox.Text
            if ([string]::IsNullOrWhiteSpace($target)) { return }
        
            $controls.View2.Visibility = "Collapsed"
            $controls.View3.Visibility = "Visible"
            $controls.BackBtn.Visibility = "Collapsed"
            $controls.NextBtn.Visibility = "Collapsed"
            Set-ActiveStep 3
            $script:CurrentStep = 3
            DoEvents
        
            # --- INSTALLATION PROCESS ---
            try {
                # 1. Prep
                $controls.StatusText.Text = "Removing old version..."
                $controls.ProgressBar.Value = 10
                DoEvents
                if (Test-Path $target) {
                    if ((Test-Path "$target\$AppExe") -or ((Get-ChildItem $target).Count -eq 0) -or ($target -eq $DefaultTargetDir)) {
                        Remove-Item -Recurse -Force $target -ErrorAction SilentlyContinue
                    }
                }
                New-Item -ItemType Directory -Path $target -Force | Out-Null
                $controls.ProgressBar.Value = 25
            
                # 2. Copy
                $controls.StatusText.Text = "Installing files..."
                $controls.StatusDetail.Text = "Copying from $SourceDir..."
                Copy-Item -Path "$SourceDir\*" -Destination $target -Recurse -Force
                $controls.ProgressBar.Value = 60
                DoEvents
            
                # 3. Shortcuts
                $controls.StatusText.Text = "Finalizing..."
                $controls.StatusDetail.Text = "Creating shortcuts..."
                DoEvents
            
                $WshShell = New-Object -ComObject WScript.Shell
                # Desktop
                $Shortcut = $WshShell.CreateShortcut("$DesktopDir\$AppName.lnk")
                $Shortcut.TargetPath = "$target\Tray\$AppExe"
                $Shortcut.WorkingDirectory = "$target\Tray"
                $Shortcut.IconLocation = "$target\Tray\$AppExe"
                $Shortcut.Description = "Extract Images from PDF"
                $Shortcut.Save()
            
                # Start Menu
                if (-not (Test-Path $StartMenuDir)) { New-Item -ItemType Directory -Path $StartMenuDir -Force | Out-Null }
                $StartShortcut = $WshShell.CreateShortcut("$StartMenuDir\$AppName.lnk")
                $StartShortcut.TargetPath = "$target\Tray\$AppExe"
                $StartShortcut.WorkingDirectory = "$target\Tray"
                $StartShortcut.IconLocation = "$target\Tray\$AppExe"
                $StartShortcut.Save()
            
                # Uninstaller
                $UninstallScript = @"
`$AppName = "$AppName"
`$TargetDir = "$target"
`$DesktopDir = [Environment]::GetFolderPath("Desktop")
`$StartMenuDir = "$StartMenuDir"
Remove-Item -Force "`$DesktopDir\`$AppName.lnk" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "`$StartMenuDir" -ErrorAction SilentlyContinue
Get-ChildItem -Path "`$TargetDir" -Recurse | Where-Object { `$_.FullName -ne `$MyInvocation.MyCommand.Path } | Remove-Item -Recurse -Force
Start-Sleep -Seconds 1
"@
                Set-Content -Path "$target\uninstall.ps1" -Value $UninstallScript
            
                $controls.ProgressBar.Value = 100
                DoEvents
                Start-Sleep -Milliseconds 600
            
                # 4. Success Ref
                $controls.View3.Visibility = "Collapsed"
                $controls.View4.Visibility = "Visible"
                $controls.FinishBtn.Visibility = "Visible"
                $controls.FinalPathText.Text = "Location: $target"
                Set-ActiveStep 4
            
            }
            catch {
                [System.Windows.MessageBox]::Show($_.Exception.Message, "Error", "OK", "Error")
                $controls.Window.Close()
            }
        }
    })

$controls.FinishBtn.Add_Click({
        $target = $controls.PathTextBox.Text
        Start-Process "$target\Tray\$AppExe"
        $controls.Window.Close()
    })

# Launch
Set-ActiveStep 1
$controls.Window.ShowDialog() | Out-Null
