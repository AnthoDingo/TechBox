// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.IO;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Pages
{
    public partial class HomeViewModel : ObservableObject, INavigationAware
    {

        private bool _isInitialized = false;
        private IActiveDirectory _activeDirectory;

        private string _domainControler;

        [ObservableProperty]
        private IEnumerable<string> _computers = new List<string>();

        [ObservableProperty]
        private Computer _selectedCompter;

        public HomeViewModel(IActiveDirectory activeDirectory)
        {
            _activeDirectory = activeDirectory;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        public async Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                await InitializeViewModelAsync();
        }

        private async Task InitializeViewModelAsync()
        {
            Computers = await _activeDirectory.GetAllComputersAsync();
            _isInitialized = true;
        }

        public async Task GetComputer(string ComputerName)
        {
            SelectedCompter = _activeDirectory.GetComputer(ComputerName);
            if (SelectedCompter.IsOnline())
            {
                
            }
        }

        [RelayCommand]
        private void RemoteDesktop()
        {
            if (SelectedCompter == null)
                return;

            Process.Start("mstsc.exe", $"/v:{SelectedCompter.Name} /f");
        }

        [RelayCommand]
        private void RemoteControl()
        {
            if(File.Exists(@"C:\Program Files (x86)\Microsoft Configuration Manager\bin\i386\CmRcViewer.exe"))
            {
                if (SelectedCompter == null)
                    return;

                Process.Start(@"C:\Program Files (x86)\Microsoft Configuration Manager\bin\i386\CmRcViewer.exe", $@"{SelectedCompter.Name} {_domainControler}");
            }
        }
    }
}
