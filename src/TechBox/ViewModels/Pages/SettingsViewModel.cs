// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Win32;
using TechBox.Controls;
using TechBox.Databases;
using TechBox.Models;
using TechBox.Statics;
using TechBox.Views.Windows;

namespace TechBox.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
	{
        private bool _isInitialized = false;
        private SQLiteContext _context = new SQLiteContext();
        private Setting _theme;
        private Setting _backupPathSetting;
        private Setting _ldapPathSetting;
        private Setting _remoteAdminUsernameSetting;
        private Setting _remoteAdminPasswordSetting;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private Wpf.Ui.Appearance.ApplicationTheme _currentTheme = Wpf.Ui.Appearance.ApplicationTheme.Unknown;

        [ObservableProperty]
        private string _backupPath = string.Empty;

        [ObservableProperty]
        private string _ldapPath = string.Empty;

        [ObservableProperty]
        private string _remoteAdminUsername = string.Empty;

        [ObservableProperty]
        private string _remoteAdminPassword = string.Empty;

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        public async Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            
            AppVersion = $"TechBox - {GetAssemblyVersion()}";

			SCCMActions = _context.SCCMActions.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();

            _backupPathSetting = _context.Settings.FirstOrDefault(s => s.Name == "backup_path");
            BackupPath = _backupPathSetting?.Value ?? string.Empty;

            _ldapPathSetting = _context.Settings.FirstOrDefault(s => s.Name == "ldap_path");
            LdapPath = _ldapPathSetting?.Value ?? string.Empty;

            _remoteAdminUsernameSetting = _context.Settings.FirstOrDefault(s => s.Name == "remote_admin_username");
            RemoteAdminUsername = _remoteAdminUsernameSetting?.Value ?? string.Empty;

            _remoteAdminPasswordSetting = _context.Settings.FirstOrDefault(s => s.Name == "remote_admin_password");
            RemoteAdminPassword = Security.Unprotect(_remoteAdminPasswordSetting?.Value ?? string.Empty);

            //_theme = _context.Settings.First(s => s.Name == "theme");
            //switch (_theme.Value)
            //{
            //    case "Unknown":
            //        CurrentTheme = Wpf.Ui.Appearance.ApplicationTheme.Unknown;
            //        break;
            //    case "Light":
            //        CurrentTheme = Wpf.Ui.Appearance.ApplicationTheme.Light;
            //        break;
            //    case "Dark":
            //        CurrentTheme = Wpf.Ui.Appearance.ApplicationTheme.Dark;
            //        break;
            //}
			//CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
			//CurrentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();

			_isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            Wpf.Ui.Appearance.ApplicationTheme theme;
            switch (parameter)
            {
                default:
                case "theme_system":
                    theme = Wpf.Ui.Appearance.ApplicationTheme.Unknown;                   
                    break;
                case "theme_light":
                    theme = Wpf.Ui.Appearance.ApplicationTheme.Light;
                    break;

                case "theme_dark":
                    theme = Wpf.Ui.Appearance.ApplicationTheme.Dark;
                    break;
            }

            if (CurrentTheme == theme)
                return;

			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(theme);
			CurrentTheme = theme;
            _theme.Value = theme.ToString();
            _context.SaveChangesAsync();
		}

        [ObservableProperty]
        private IEnumerable<SCCMAction> _sCCMActions;


        [RelayCommand]
        private void SCCMActionToggle(SCCMAction action)
        {
            using (SQLiteContext db = new SQLiteContext())
            {
                db.Attach(action);
                db.Entry(action)
                    .Property(a => a.IsEnabled)
                    .IsModified = true;

                db.SaveChanges();
            }

            CCMCard.InvalidateActionsCache();
        }

        [RelayCommand]
        private void SaveBackupPath()
        {
            if (_backupPathSetting is null)
                return;

            _backupPathSetting.Value = BackupPath;
            _context.SaveChanges();
        }

        [RelayCommand]
        private void BrowseBackupPath()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                BackupPath = dialog.FolderName;
                SaveBackupPath();
            }
        }

        [RelayCommand]
        private void SaveLdapPath()
        {
            if (_ldapPathSetting is null)
                return;

            _ldapPathSetting.Value = LdapPath;
            _context.SaveChanges();
        }

        [RelayCommand]
        private void BrowseLdapPath()
        {
            AdExplorerWindow window = new(LdapPath)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true && window.ViewModel.SelectedNode is not null)
            {
                // ActiveDirectoryService binds a DirectoryEntry directly from this value, so it must be
                // a full ADsPath ("LDAP://...") and not a bare distinguished name.
                LdapPath = window.ViewModel.SelectedNode.Path;
                SaveLdapPath();
            }
        }

        [RelayCommand]
        private void SaveRemoteAdminCredentials()
        {
            if (_remoteAdminUsernameSetting is null || _remoteAdminPasswordSetting is null)
                return;

            _remoteAdminUsernameSetting.Value = RemoteAdminUsername;
            // DPAPI-encrypted (CurrentUser scope) - only readable by this Windows account on this machine.
            _remoteAdminPasswordSetting.Value = Security.Protect(RemoteAdminPassword);
            _context.SaveChanges();
        }
	}
}
