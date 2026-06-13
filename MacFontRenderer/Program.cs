using System;
using System.Security.Principal;
using System.Windows;
using MacFontRenderer.Utils;

namespace MacFontRenderer
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Check for admin privileges
            if (!IsRunAsAdmin())
            {
                MessageBox.Show(
                    "This application requires Administrator privileges.\n\n" +
                    "Please run it with elevated rights to proceed.",
                    "Admin Privileges Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                System.Environment.Exit(1);
            }

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static bool IsRunAsAdmin()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
