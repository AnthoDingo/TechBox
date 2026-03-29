// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using TechBox.Databases;
using TechBox.Models;

namespace TechBox.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
	{
        private bool _isInitialized = false;
        private SQLiteContext _context = new SQLiteContext();
        private Setting _theme;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private Wpf.Ui.Appearance.ApplicationTheme _currentTheme = Wpf.Ui.Appearance.ApplicationTheme.Unknown;

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
        }
	}
}
