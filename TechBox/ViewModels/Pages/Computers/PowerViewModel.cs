using System.Threading;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class PowerViewModel : ObservableObject, INavigationAware
    {
        private const int RefreshIntervalMs = 10_000;

        private bool _isInitialized = false;

        private readonly IActiveDirectory _activeDirectory;

        private Timer? _uptimeTimer;

        [ObservableProperty]
        private IEnumerable<string> _computers = new List<string>();

        [ObservableProperty]
        private Computer _selectedCompter = new Computer();

        [ObservableProperty]
        private bool _hasSelection = false;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public PowerViewModel(IActiveDirectory activeDirectory)
        {
            _activeDirectory = activeDirectory;
        }

        public Task OnNavigatedFromAsync()
        {
            StopUptimeRefresh();
            return Task.CompletedTask;
        }

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
            StopUptimeRefresh();

            SelectedCompter = await Task.Run(() => _activeDirectory.GetComputer(ComputerName));
            HasSelection = true;
            StatusMessage = string.Empty;

            if (!await Task.Run(() => SelectedCompter.IsOnline()))
            {
                StatusMessage = "Poste injoignable.";
                return;
            }

            await RefreshUptimeAsync();

            // Ticks on a ThreadPool thread separate from the UI thread: each tick re-queries WMI
            // and updates SelectedCompter, keeping the displayed uptime current every 10 seconds.
            _uptimeTimer = new Timer(async _ => await RefreshUptimeAsync(), null, RefreshIntervalMs, RefreshIntervalMs);
        }

        private async Task RefreshUptimeAsync()
        {
            Computer computer = SelectedCompter;
            try
            {
                await computer.GetUptime();
                if (ReferenceEquals(SelectedCompter, computer))
                    StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                if (ReferenceEquals(SelectedCompter, computer))
                    StatusMessage = $"Erreur : {ex.Message}";
            }

            if (ReferenceEquals(SelectedCompter, computer))
                OnPropertyChanged(nameof(SelectedCompter));
        }

        private void StopUptimeRefresh()
        {
            _uptimeTimer?.Dispose();
            _uptimeTimer = null;
        }
    }
}
