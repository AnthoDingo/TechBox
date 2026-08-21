using System.Windows;
using TechBox.Services;

namespace TechBox.Views;

public partial class SettingsWindow : Window
{
    private readonly ISettingsService _settingsService;

    public SettingsWindow(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeComponent();
        RefreshLdapPathDisplay();
    }

    private void RefreshLdapPathDisplay()
    {
        var settings = _settingsService.Load();
        LdapPathTextBox.Text = string.IsNullOrWhiteSpace(settings.LdapDistinguishedName)
            ? "Aucun emplacement sélectionné"
            : settings.LdapDistinguishedName;
    }

    private void BrowseLdapPathButton_Click(object sender, RoutedEventArgs e)
    {
        var selectionWindow = new LdapPathSelectionWindow(_settingsService, isMandatory: false) { Owner = this };
        selectionWindow.ShowDialog();
        RefreshLdapPathDisplay();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
