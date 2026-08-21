using System.Windows;
using TechBox.Services;
using TechBox.Views;

namespace TechBox;

public partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;

    public MainWindow() : this(new SettingsService())
    {
    }

    public MainWindow(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeComponent();
        RefreshLdapPathDisplay();
    }

    private void RefreshLdapPathDisplay()
    {
        var settings = _settingsService.Load();
        LdapPathTextBlock.Text = string.IsNullOrWhiteSpace(settings.LdapDistinguishedName)
            ? "Aucun emplacement LDAP sélectionné."
            : $"Emplacement LDAP actif : {settings.LdapDistinguishedName}";
    }

    private void ChangeLdapPath_Click(object sender, RoutedEventArgs e)
    {
        var selectionWindow = new LdapPathSelectionWindow(_settingsService, isMandatory: false) { Owner = this };
        selectionWindow.ShowDialog();
        RefreshLdapPathDisplay();
    }

    private void Quit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
