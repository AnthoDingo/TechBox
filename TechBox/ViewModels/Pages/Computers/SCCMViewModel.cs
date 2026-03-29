
using System.Collections.ObjectModel;
using TechBox.Controls;
using TechBox.Databases;
using TechBox.Models;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;
using TechBox.Statics;
using Wpf.Ui.Controls;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class SCCMViewModel : ObservableObject, INavigationAware
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

        internal void SetComputer(string ComputerName)
        {
            SelectedCompter = _activeDirectory.GetComputer(ComputerName);
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