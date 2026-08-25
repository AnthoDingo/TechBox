using System.Collections.ObjectModel;
using TechBox.Controls;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class PowerViewModel : ObservableObject, INavigationAware
    {

        private bool _isInitialized = false;

        private readonly IActiveDirectory _activeDirectory;

        [ObservableProperty]
        private IEnumerable<string> _computers = new List<string>();

        [ObservableProperty]
        private ObservableCollection<PowerCard> _powerCards = new ObservableCollection<PowerCard>();

        public PowerViewModel(IActiveDirectory activeDirectory)
        {
            _activeDirectory = activeDirectory;
        }

        public Task OnNavigatedFromAsync()
        {
            foreach (PowerCard card in PowerCards)
                card.StopTracking();

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

        public void GetComputer(string ComputerName)
        {
            // A card already tracks this computer - leave it as is instead of replacing it.
            if (PowerCards.Any(c => string.Equals(c.ComputerName, ComputerName, StringComparison.OrdinalIgnoreCase)))
                return;

            PowerCard card = new PowerCard { ComputerName = ComputerName };
            PowerCards.Add(card);
            _ = card.StartTracking();
        }
    }
}
