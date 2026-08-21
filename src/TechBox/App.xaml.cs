using System.Windows;
using TechBox.Services;
using TechBox.Views;

namespace TechBox;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsService = new SettingsService();

        // First launch (or any launch without a configured location): the LDAP path
        // selection window is mandatory and blocks the rest of the application.
        if (string.IsNullOrWhiteSpace(settingsService.Load().LdapPath))
        {
            var selectionWindow = new LdapPathSelectionWindow(settingsService, isMandatory: true);
            var confirmed = selectionWindow.ShowDialog();

            if (confirmed != true || string.IsNullOrWhiteSpace(settingsService.Load().LdapPath))
            {
                // The user closed the mandatory window without selecting a location: exit the app.
                Shutdown();
                return;
            }
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
