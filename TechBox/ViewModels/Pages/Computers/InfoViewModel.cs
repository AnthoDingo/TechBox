using System.ComponentModel;
using System.Diagnostics;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class InfoViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
    {

        private bool _isInitialized = false;

        private IActiveDirectory _activeDirectory;

        [ObservableProperty]
        private IEnumerable<string> _computers = new List<string>();

        [ObservableProperty]
        private Computer _selectedCompter = new Computer();

        public InfoViewModel(IActiveDirectory activeDirectory)
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
            Debug.WriteLine(ComputerName);
            SelectedCompter = await Task.Run(() => _activeDirectory.GetComputer(ComputerName));
            if (await Task.Run(() => SelectedCompter.IsOnline()))
            {
                await Task.WhenAll(
                    Task.Run(GetHardware),
                    Task.Run(GetDisks),
                    Task.Run(GetSoftwares),
                    Task.Run(GetConnectedUser)
                );
            }
        }

        [ObservableProperty]
        private bool _searchingHardware = false;
        private async Task GetHardware()
        {
            SearchingHardware = true;
            await SelectedCompter.GetHardware();
            OnPropertyChanged(nameof(SelectedCompter));
            SearchingHardware = false;
        }

        private async Task GetConnectedUser()
        {
            await SelectedCompter.GetConnectedUser();
            OnPropertyChanged(nameof(SelectedCompter));
            //ConnectedUser = SelectedCompter.GetConnectedUser().ToString();
        }

        [ObservableProperty]
        private bool _searchingDisks = false;
        private async Task GetDisks()
        {
            SearchingDisks = true;
            await SelectedCompter.GetLogicalDisks();
            OnPropertyChanged(nameof(SelectedCompter));
            SearchingDisks = false;
        }

        [ObservableProperty]
        private bool _searchingSoftwares = false;
        private async Task GetSoftwares()
        {
            SearchingSoftwares = true;
            //await SelectedCompter.GetSoftwares();
            OnPropertyChanged(nameof(SelectedCompter));
            SearchingSoftwares = false;
        }
    }
}
