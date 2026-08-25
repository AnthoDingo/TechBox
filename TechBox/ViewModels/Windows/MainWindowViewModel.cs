// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TechBox.Databases;
using TechBox.Models;
using TechBox.PluginContract;
using TechBox.Services.Contracts;
using TechBox.Views.Windows;
using Wpf.Ui.Controls;

namespace TechBox.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {

        private readonly IActiveDirectory _activeDirectory;
        private readonly IReadOnlyList<ITechBoxPlugin> _plugins;

        [ObservableProperty]
        private string _applicationTitle = "Local IT TechBox";

        [ObservableProperty]
        private ImageSource _logoSource = new BitmapImage(new Uri("pack://application:,,,/Assets/wpfui-icon-256.png"));

        [ObservableProperty]
        private ObservableCollection<object> _menuItems;

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };


        public Action<int, int>? UpdateWindowDimension;

        [ObservableProperty]
        private Visibility _splashScreenVisbility = Visibility.Visible;

        [ObservableProperty]
        private Visibility _initialSetupVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility _mainContentVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string _loadingStatus = "Loading...";

        public MainWindowViewModel(IActiveDirectory activeDirectory, IReadOnlyList<ITechBoxPlugin> plugins)
        {
            _activeDirectory = activeDirectory;
            _plugins = plugins;

            //            if (System.IO.File.Exists(@"C:\Program Files\One Identity\Active Roles\7.3\Console\ActiveRoles.msc"))
            //            {
            //#if !DEBUG
            //                _adConsoleItem.Click += (_, _) =>
            //                {
            //                    Process.Start("mmc.exe", "\"C:\\Program Files\\One Identity\\Active Roles\\7.3\\Console\\ActiveRoles.msc\"");
            //                };
            //#endif
            //#if DEBUG
            //                _adConsoleItem.TargetPageType = typeof(Views.Pages.Tools.ActiveDirectoryPage);
            //#endif
            //            } else
            //            {

            //            }

            NavigationViewItem ccmConsole;
            if (System.IO.File.Exists(@"C:\Program Files (x86)\Microsoft Configuration Manager\bin\Microsoft.ConfigurationManagement.exe"))
            {
                ccmConsole = CreateNavigationViewItem("CCM Console", SymbolRegular.WindowConsole20, @"C:\Program Files (x86)\Microsoft Configuration Manager\bin\Microsoft.ConfigurationManagement.exe");
            }

            MenuItems = new();
            MenuItems.Add(new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.HomePage)
            });
            MenuItems.Add(new NavigationViewItemSeparator());


            // Users
            NavigationViewItem users = new NavigationViewItem()
            {
                Content = "Users",
                Icon = new SymbolIcon { Symbol = SymbolRegular.PeopleToolbox20 },               
            };
            users.MenuItems.Add(new NavigationViewItem("Infos", SymbolRegular.PeopleSearch20, typeof(Views.Pages.Users.InfoPage)));
            MenuItems.Add(users);

            NavigationViewItem computers = new NavigationViewItem()
            {
                Content = "Computers",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DesktopToolbox20 },
            };
            computers.MenuItems.Add(new NavigationViewItem("Computer Infos", SymbolRegular.DeveloperBoardSearch20, typeof(Views.Pages.Computers.InfoPage)));
            computers.MenuItems.Add(new NavigationViewItem("SCCM", SymbolRegular.ClipboardTaskListLtr20, typeof(Views.Pages.Computers.SCCMPage)));
            computers.MenuItems.Add(new NavigationViewItem("Backup Profile", SymbolRegular.SaveArrowRight20, typeof(Views.Pages.Computers.BackupProfilePage)));
            //computers.MenuItems.Add(new NavigationViewItem("Restore Profile", SymbolRegular.SaveEdit20, typeof(Views.Pages.Computers.RestoreProfilePage)));
            computers.MenuItems.Add(new NavigationViewItem("GLPI", SymbolRegular.Box20, typeof(Views.Pages.Computers.InfoPage)));
            computers.MenuItems.Add(new NavigationViewItem("Power", SymbolRegular.Power20, typeof(Views.Pages.Computers.PowerPage)));
            MenuItems.Add(computers);

            NavigationViewItem tools = new NavigationViewItem()
            {
                Content = "Tools",
                Icon = new SymbolIcon { Symbol = SymbolRegular.WindowDevTools20 },
            };
            tools.MenuItems.Add(new NavigationViewItem("Active Directory",SymbolRegular.BookContacts20, typeof(Views.Pages.Tools.ActiveDirectoryPage)));
            tools.MenuItems.Add(new NavigationViewItem("PowerShell", SymbolRegular.WindowConsole20, typeof(Views.Pages.Tools.PowerShellPage)));
            if (System.IO.File.Exists(@"C:\Program Files (x86)\Microsoft Configuration Manager\bin\Microsoft.ConfigurationManagement.exe"))
            {
                tools.MenuItems.Add(CreateNavigationViewItem("CCM Console", SymbolRegular.WindowConsole20, @"C:\Program Files (x86)\Microsoft Configuration Manager\bin\Microsoft.ConfigurationManagement.exe"));
            }
            tools.MenuItems.Add(CreateNavigationViewItem("MMC", SymbolRegular.WindowConsole20, "mmc.exe"));
            MenuItems.Add(tools);

            ApplyPlugins();
        }

        private void ApplyPlugins()
        {
            foreach (ITechBoxPlugin plugin in _plugins)
            {
                foreach (NavigationViewItem item in plugin.CreateMenuItems())
                {
                    MenuItems.Add(item);
                }

                if (!string.IsNullOrWhiteSpace(plugin.ApplicationTitle) && ApplicationTitle == "Local IT TechBox")
                {
                    ApplicationTitle = plugin.ApplicationTitle;
                }

                if (!string.IsNullOrWhiteSpace(plugin.LogoImagePath) && System.IO.File.Exists(plugin.LogoImagePath))
                {
                    LogoSource = new BitmapImage(new Uri(plugin.LogoImagePath, UriKind.Absolute));
                }
            }
        }

        public async Task Loaded()
        {
            UpdateWindowDimension?.Invoke(100, 600);
            using (SQLiteContext db = new SQLiteContext())
            {
                bool isInitialSetup = Convert.ToBoolean(db.Settings.First(s => s.Name.Equals("isInitialSetup")).Value);
                if (isInitialSetup)
                {
                    await InvokeInitialSetupAsync();
                }
                else
                {
                    await InvokeSpashScreenAsync();
                }
            }
        }

        private async Task InvokeInitialSetupAsync()
        {
            InitialSetupVisibility = Visibility.Visible;
            LoadingStatus = "Configuration initiale : sélection de l'emplacement LDAP...";

            AdExplorerWindow window = new(initialLdapPath: null, isMandatory: true)
            {
                Owner = Application.Current.MainWindow
            };

            string? selectedLdapPath = window.ShowDialog() == true
                ? window.ViewModel.SelectedNode?.Path
                : null;

            using (SQLiteContext db = new SQLiteContext())
            {
                Setting ldapSetting = db.Settings.First(s => s.Name.Equals("ldap_path"));
                ldapSetting.Value = selectedLdapPath ?? string.Empty;

                Setting initialSetupSetting = db.Settings.First(s => s.Name.Equals("isInitialSetup"));
                initialSetupSetting.Value = "false";

                await db.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(selectedLdapPath))
            {
                _activeDirectory.SetLdapPath(selectedLdapPath);
            }

            InitialSetupVisibility = Visibility.Collapsed;

            await InvokeSpashScreenAsync();
        }

        private async Task InvokeSpashScreenAsync()
        {
            LoadingStatus = "Loading computers from Active Directory ...";
            await _activeDirectory.GetAllComputersAsync(true);

            LoadingStatus = "Loading users from Active Directory ...";
            await _activeDirectory.GetAllUsersAsync(true);

            SplashScreenVisbility = Visibility.Collapsed;
            MainContentVisibility = Visibility.Visible;

            UpdateWindowDimension?.Invoke(1400, 650);
        }

        private NavigationViewItem CreateNavigationViewItem(string content, SymbolRegular icon, string process)
        {
            NavigationViewItem item = new NavigationViewItem()
            {
                Content = content,
                Icon = new SymbolIcon { Symbol = icon }
            };
            item.Click += (_, _) => Process.Start(process);
            return item;
        }
    }
}
