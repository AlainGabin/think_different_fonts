using System;
using System.Windows;
using System.Windows.Media;
using MacFontRenderer.Services;

namespace MacFontRenderer.Views
{
    public partial class MainWindow : Window
    {
        private RegistryService _registryService;
        private FontService _fontService;

        public MainWindow()
        {
            InitializeComponent();
            _registryService = new RegistryService();
            _fontService = new FontService();
            
            GammaSlider.ValueChanged += (s, e) =>
            {
                GammaValue.Text = ((int)e.NewValue).ToString();
            };

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckSystemState();
        }

        private void CheckSystemState()
        {
            try
            {
                LogMessage("Checking system state...");
                
                // Check if optimization is active
                bool isActive = _registryService.IsOptimizationActive();
                StatusText.Text = isActive ? "Optimization active on this system" : "No optimization applied yet";
                StatusIndicator.Fill = isActive ? 
                    new SolidColorBrush(Color.FromRgb(52, 199, 89)) :  // Green
                    new SolidColorBrush(Color.FromRgb(200, 200, 205)); // Gray

                // Check installed fonts
                if (_fontService.IsFontInstalled("SF Pro Text"))
                {
                    LogMessage("✓ SF Pro Text font detected");
                    FontComboBox.SelectedIndex = 0;
                }
                else if (_fontService.IsFontInstalled("Inter"))
                {
                    LogMessage("✓ Inter font detected");
                    FontComboBox.SelectedIndex = 1;
                }
                else
                {
                    LogMessage("⚠ No target font detected. Consider installing SF Pro Text or Inter.");
                }

                // Load current Gamma value
                int currentGamma = _registryService.GetCurrentGamma();
                GammaSlider.Value = currentGamma;
                GammaValue.Text = currentGamma.ToString();

                LogMessage("System check complete.");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex.Message}");
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 59, 48)); // Red
                StatusText.Text = "Error reading system state";
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LogMessage("\n━━━ Applying macOS Font Style ━━━");
                int gammaValue = (int)GammaSlider.Value;
                string targetFont = FontComboBox.SelectedIndex == 0 ? "SF Pro Text" : "Inter";

                LogMessage($"Target: {targetFont} | Gamma: {gammaValue}");
                LogMessage("...");

                // 1. Apply GDI tweaks (HKCU)
                LogMessage("① Applying GDI tweaks...");
                _registryService.ApplyGDITweaks(gammaValue);
                LogMessage("✓ GDI tweaks applied");

                // 2. Apply environment variables (HKLM)
                LogMessage("② Injecting DirectWrite environment...");
                _registryService.ApplyDirectWriteEnvironment();
                LogMessage("✓ DirectWrite configured");

                // 3. Broadcast WM_SETTINGCHANGE
                LogMessage("③ Broadcasting system change...");
                _registryService.BroadcastSettingChange();
                LogMessage("✓ Setting change broadcast");

                // 4. Map fonts in HKLM
                LogMessage("④ Mapping system fonts...");
                _registryService.MapSystemFonts(targetFont);
                LogMessage("✓ Font mappings updated");

                // 5. Deploy fonts to C:\Windows\Fonts
                LogMessage("⑤ Deploying SF Pro Text fonts...");
                _registryService.DeployFonts();
                LogMessage("✓ Fonts deployed");

                // 6. Deploy Layer 4 (AppInit_DLLs hooks)
                LogMessage("⑥ Deploying AppInit_DLLs hooks...");
                _registryService.DeployAppInitHooks();
                LogMessage("✓ AppInit hooks deployed");

                LogMessage("\n✅ Optimization Complete (All 4 Layers)");
                LogMessage("Restart required for AppInit hooks to activate.");
                
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(52, 199, 89)); // Green
                StatusText.Text = "Optimization active on this system";

                MessageBox.Show(
                    "macOS font style applied successfully!\n\nPlease restart your computer for all changes to take full effect.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"❌ ERROR: {ex.Message}");
                MessageBox.Show($"Error applying changes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRestoreClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "Restore all Windows font settings to defaults?\n\nThis action is reversible.",
                    "Confirm Restore",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                LogMessage("\n━━━ Restoring Windows Defaults ━━━");

                LogMessage("① Restoring GDI settings...");
                _registryService.RestoreGDIDefaults();
                LogMessage("✓ GDI restored");

                LogMessage("② Removing DirectWrite environment...");
                _registryService.RemoveDirectWriteEnvironment();
                LogMessage("✓ DirectWrite cleared");

                LogMessage("③ Restoring default font mappings...");
                _registryService.RestoreDefaultFontMappings();
                LogMessage("✓ Font mappings restored");

                LogMessage("④ Removing AppInit_DLLs hooks...");
                _registryService.RemoveAppInitHooks();
                LogMessage("✓ AppInit hooks removed");

                LogMessage("\n✅ Restore Complete (All Layers)");
                LogMessage("Restart recommended.");

                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(200, 200, 205)); // Gray
                StatusText.Text = "No optimization applied yet";

                MessageBox.Show(
                    "Windows defaults restored successfully!\n\nAll 4 layers have been removed.\nPlease restart your computer.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"❌ ERROR: {ex.Message}");
                MessageBox.Show($"Error restoring defaults:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = string.IsNullOrWhiteSpace(message) ? "\n" : $"[{timestamp}] {message}\n";
            LogOutput.AppendText(logEntry);
            LogOutput.ScrollToEnd();
        }
    }
}
