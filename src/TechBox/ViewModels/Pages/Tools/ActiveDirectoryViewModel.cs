using System.ComponentModel;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Pages.Tools
{
    public partial class ActiveDirectoryViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
    {

        private bool _isInitialized = false;
        private IActiveDirectory _activeDirectory;

        [ObservableProperty]
        private IEnumerable<string> _computers;

        [ObservableProperty]
        private IEnumerable<string> _users;

        public ActiveDirectoryViewModel(IActiveDirectory activeDirectory) { 
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
            Task<IEnumerable<string>> computersTask = _activeDirectory.GetAllComputersAsync();
            Task<IEnumerable<string>> usersTask = _activeDirectory.GetAllUsersAsync();
            await Task.WhenAll(computersTask, usersTask);

            Computers = computersTask.Result;
            Users = usersTask.Result;
        }
    }
}
