using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechBox.Models;
using TechBox.Services;

namespace TechBox.Views;

/// <summary>
/// Lets the user browse the domain's LDAP tree and pick a location.
/// When <see cref="_isMandatory"/> is true (first launch), the window cannot be
/// dismissed without a confirmed selection: closing it without confirming asks
/// whether to exit the whole application instead.
/// </summary>
public partial class LdapPathSelectionWindow : Window
{
    private readonly ISettingsService _settingsService;
    private readonly ILdapTreeService _ldapTreeService;
    private readonly bool _isMandatory;
    private bool _confirmed;
    private LdapNode? _selectedNode;

    public LdapPathSelectionWindow(ISettingsService settingsService, bool isMandatory)
        : this(settingsService, new LdapTreeService(), isMandatory)
    {
    }

    public LdapPathSelectionWindow(ISettingsService settingsService, ILdapTreeService ldapTreeService, bool isMandatory)
    {
        _settingsService = settingsService;
        _ldapTreeService = ldapTreeService;
        _isMandatory = isMandatory;

        InitializeComponent();

        if (!_isMandatory)
        {
            DescriptionTextBlock.Text =
                "Choisissez l'unité d'organisation (OU) ou le conteneur que TechBox utilisera par défaut, puis confirmez votre sélection.";
        }

        Loaded += async (_, _) => await LoadRootAsync();
    }

    private async Task LoadRootAsync()
    {
        StatusTextBlock.Text = string.Empty;
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            var root = await Task.Run(() => _ldapTreeService.GetRootNode());
            LdapTreeView.Items.Clear();
            LdapTreeView.Items.Add(root);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text =
                $"Impossible de contacter l'annuaire LDAP du domaine. Vérifiez que ce poste est bien joint à un domaine et réessayez. ({ex.Message})";
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem { DataContext: LdapNode node })
        {
            return;
        }

        if (node.ChildrenLoaded || !node.HasChildren)
        {
            return;
        }

        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            var children = await Task.Run(() => _ldapTreeService.GetChildren(node));
            node.Children.Clear();
            foreach (var child in children)
            {
                node.Children.Add(child);
            }

            node.ChildrenLoaded = true;
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Impossible de charger le contenu de « {node.Name} ». ({ex.Message})";
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        e.Handled = true;
    }

    private void LdapTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedNode = e.NewValue as LdapNode;
        SelectedPathTextBlock.Text = _selectedNode?.DistinguishedName is { Length: > 0 } dn ? dn : "(aucun)";
        ConfirmButton.IsEnabled = _selectedNode is not null;
        StatusTextBlock.Text = string.Empty;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedNode is null)
        {
            return;
        }

        var settings = _settingsService.Load();
        settings.LdapPath = _selectedNode.Path;
        settings.LdapDistinguishedName = _selectedNode.DistinguishedName;
        _settingsService.Save(settings);

        _confirmed = true;
        DialogResult = true;
        Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (_confirmed || !_isMandatory)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            "La sélection d'un emplacement LDAP est obligatoire pour utiliser TechBox. Voulez-vous quitter l'application ?",
            "Sélection obligatoire",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
        else
        {
            e.Cancel = true;
        }
    }
}
