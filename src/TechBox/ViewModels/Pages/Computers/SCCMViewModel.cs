
using System.Collections.ObjectModel;
using TechBox.Controls;
using TechBox.Databases;
using TechBox.Models;
using TechBox.Models.Hardware;
using TechBox.PluginContract;
using TechBox.Services.Contracts;
using TechBox.Statics;
using Wpf.Ui.Controls;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class SCCMViewModel : ObservableObject, INavigationAware, IExternalWindowState
    {

        private bool _isInitialized = false;

        private IActiveDirectory _activeDirectory;

        [ObservableProperty]
        private IEnumerable<string> _computers = new List<string>();

        [ObservableProperty]
        private Computer _selectedCompter = new Computer();

        [ObservableProperty]
        private ObservableCollection<CCMCard> _cCMCards = new ObservableCollection<CCMCard>();
        
        public SCCMViewModel(IActiveDirectory activeDirectory)
        {
            _activeDirectory = activeDirectory;
        }

        /// <summary>
        /// Carries over the selected computer only. The cards are left behind on purpose: recreating
        /// one runs its SCCM actions against the machine, and opening a window is no reason to fire
        /// them a second time.
        /// </summary>
        public async Task CopyStateFromAsync(object source)
        {
            if (source is SCCMViewModel origin && !string.IsNullOrWhiteSpace(origin.SelectedCompter?.Name))
            {
                await SetComputer(origin.SelectedCompter.Name);
            }
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

        /// <summary>
        /// Looks the computer up on a ThreadPool thread: the directory query blocks long enough to
        /// freeze the window when run straight from the UI thread, as picking a name does.
        /// </summary>
        public async Task SetComputer(string ComputerName)
        {
            SelectedCompter = await Task.Run(() => _activeDirectory.GetComputer(ComputerName));
        }

        public bool IsEnable
        {
            get { return Security.IsAdmin(); }
        }


        [RelayCommand]
        private void AddCompter()
        {
            CCMCard card = new CCMCard() { ComputerName = SelectedCompter.Name };
            CCMCards.Add(card);
            card.RunActions();
            card.RemoveEvent += RemoveCard;
        }

        private void RemoveCard(CCMCard card)
        {
            CCMCards.Remove(card);
        }
    }
}