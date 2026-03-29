using System.ComponentModel;
using System.Windows.Controls;
using TechBox.Models.Hardware;
using TechBox.Services.Contracts;
using RoboSharp;
using TechBox.Controls;

namespace TechBox.ViewModels.Pages.Computers
{
    public partial class BackupProfileViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
    {

        private bool _isInitialized = false;
        
        private string _destination = @"G:\REDACTED-SHARE";

        [ObservableProperty]
        public List<string> backupProfiles = new List<string>();

        private IActiveDirectory _activeDirectory;

        [ObservableProperty]
        private IEnumerable<string> _computers = new List<string>();

        [ObservableProperty]
        private Computer _selectedCompter = new Computer();

        public BackupProfileViewModel(IActiveDirectory activeDirectory)
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
                Task.WhenAll(this.GetUserProfiles());
            }
        }

        private async Task<Task> GetUserProfiles()
        {
            return Task.Run(async () =>
            {
                BackupProfiles = SelectedCompter.UserProfiles;
                OnPropertyChanged(nameof(SelectedCompter));
            });
        }


        #region CheckBox
        [ObservableProperty]
        private ObservableList<FolderCard> _userFolders = new ObservableList<FolderCard>()
        {
            new FolderCard(){ Title = "Desktop", IsChecked = true, FolderName = "Desktop" },
            new FolderCard() { Title = "Pictures", IsChecked = true, FolderName = "Pictures" },
            new FolderCard() { Title = "Music", IsChecked = true, FolderName = "Music" },
            new FolderCard() { Title = "Downloads", IsChecked = true, FolderName = "Downloads" },
            new FolderCard() { Title = "Videos", IsChecked = true, FolderName = "Videos" },
        };

        [ObservableProperty]
        private List<CheckBox> _localAppData = new List<CheckBox>()
        {
            new CheckBox(){Content = "Google", IsChecked = true},
        };

        [ObservableProperty]
        private List<CheckBox> _roamingAppData = new List<CheckBox>()
        {
            new CheckBox(){Content = "Outlook Signatures", IsChecked = true},
            new CheckBox(){Content = "SAP Common", IsChecked = true},
            new CheckBox(){Content = "Windows Automatic Destinations", IsChecked = true},
            new CheckBox(){Content = "Microsoft Sticky Notes", IsChecked = true},
        };

        #endregion

        [ObservableProperty]
        string _selectedProfile = string.Empty;

        private bool _isRunning = false;

        public string ButtonText
        {
            get
            {
                return !_isRunning ? "Start" : "Stop";
            }
        }
        public string ButtonApparence
        {
            get
            {
                return !_isRunning ? "Success" : "Danger";
            }
        }

        [RelayCommand]
        async Task RunCopy()
        {
            _isRunning = !_isRunning;
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(ButtonApparence));

            if( _isRunning )
            {
                foreach (FolderCard folderCard in UserFolders)
                {
                    folderCard.PrepareCopy($@"\\{SelectedCompter.Name}\c$\Users\{SelectedProfile}", $@"{_destination}\{SelectedProfile}");
                }

                _ = Task.Run(async () =>
                {
                    foreach (FolderCard folderCard in UserFolders)
                    {
                        await folderCard.RunCopy();
                    }

                    foreach (FolderCard folderCard in UserFolders)
                    {
                        folderCard.Release();
                    }
                });
            } else
            {
                foreach(FolderCard folderCard in UserFolders)
                {
                    folderCard.Cancel();
                    folderCard.Release();
                }
            }
        }      
    }
}
