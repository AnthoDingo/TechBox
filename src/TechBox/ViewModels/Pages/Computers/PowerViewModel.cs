using System.Collections.ObjectModel;
using TechBox.Controls;
using TechBox.PluginContract;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class PowerViewModel : ObservableObject, INavigationAware, IExternalWindowState
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

        /// <summary>
        /// Recreates a card per computer the main window tracks. Each detached card does its own
        /// tracking, which is a read of the machine's state and nothing more - no power action is
        /// replayed.
        /// </summary>
        public Task CopyStateFromAsync(object source)
        {
            if (source is not PowerViewModel origin)
            {
                return Task.CompletedTask;
            }

            foreach (PowerCard card in origin.PowerCards.ToList())
            {
                if (!string.IsNullOrWhiteSpace(card.ComputerName))
                {
                    GetComputer(card.ComputerName);
                }
            }

            return Task.CompletedTask;
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
            card.RemoveEvent += RemoveCard;
            PowerCards.Add(card);
            _ = card.StartTracking();
        }

        private void RemoveCard(PowerCard card)
        {
            card.RemoveEvent -= RemoveCard;
            PowerCards.Remove(card);
        }
    }
}
