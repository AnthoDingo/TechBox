using System.ComponentModel;
using System.Windows.Markup;
using TechBox.Models.ActiveDirectory;
using TechBox.PluginContract;
using TechBox.Services.Contracts;
using TechBox.ViewModels.Windows;
using TechBox.Views.Windows;
using Wpf.Ui.Controls;

namespace TechBox.ViewModels.Pages.Users
{
    public partial class InfoViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged, IExternalWindowState
    {
        private bool _isInitialized = false;
        private IActiveDirectory _activeDirectory;

        public InfoViewModel(IActiveDirectory activeDirectory)
        {
            _activeDirectory = activeDirectory;
        }

        /// <summary>
        /// Re-runs the lookup for the user the main window is displaying, rather than copying the
        /// <see cref="User"/> across: the model wraps live directory objects, which the detached
        /// window is better off holding its own.
        /// </summary>
        public async Task CopyStateFromAsync(object source)
        {
            if (source is not InfoViewModel origin || origin.SelectedUser is null)
            {
                return;
            }

            Task lookup = await GetUser(origin.SelectedUser.samAccountName);
            await lookup;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        public async Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                await InitializeViewModelAsync();
        }

        private async Task InitializeViewModelAsync()
        {
            Users = await _activeDirectory.GetAllUsersAsync();
            _isInitialized = true;
        }

        [ObservableProperty]
        private IEnumerable<string> _users = new List<string>();

        public void SearchUsers(string username)
        {
            Users = _activeDirectory.SearchUsers(username);
            OnPropertyChanged(nameof(Users));
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLockedString))]
        [NotifyPropertyChangedFor(nameof(IsDisabledString))]
        private User _selectedUser;

        #region User Infos
        [ObservableProperty]
        private bool _searchingInfos = false;

        public async Task<Task> GetUser(string username)
        {
            Task getUser = Task.Run(() =>
            {
                SearchingInfos = true;
                SearchingMemberOf = true;
                MemberOfSearch = string.Empty;

                SelectedUser = _activeDirectory.GetUser(username);
                SearchingInfos = false;
            });

            Task getGroups = getUser.ContinueWith((antecedent) =>
            {
                
                AllMemberOf = SelectedUser.MemberOf.OrderBy(g => g.Name);
                SearchingMemberOf = false;
            });
            //Task.WaitAll(getUser, getGroups);

            return getUser;
        }

        public string IsLockedString
        {
            get
            {
                if (SelectedUser == null)
                    return "Unlocked";

                return (bool)SelectedUser.IsLocked ? "Locked" : "Unlocked";
            }
        }

        public string IsDisabledString
        {
            get
            {
                if (SelectedUser == null)
                    return "Enabled";

                return (bool)SelectedUser.IsDisabled ? "Disabled" : "Enabled";
            }
        }

        #endregion

        #region MemberOf
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SearchingMemberOfInverted))]
        private bool _searchingMemberOf = false;

        public bool SearchingMemberOfInverted
        {
            get
            {
                return !SearchingMemberOf;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MemberOf))]
        private IEnumerable<Group> _allMemberOf;

        public IEnumerable<Group> MemberOf
        {
            get
            {
                if (string.IsNullOrEmpty(MemberOfSearch))
                    return AllMemberOf;

                return AllMemberOf.Where(g => g.Name.ToLower().Contains(MemberOfSearch.ToLower()));
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MemberOf))]
        private string _memberOfSearch = string.Empty;

        /// <summary>Opens a non-blocking window listing the members of <paramref name="group"/>.</summary>
        public void ShowGroupMembers(Group group)
        {
            if (group is null || string.IsNullOrEmpty(group.DistinguishedName))
                return;

            GroupMembersWindow window = new GroupMembersWindow(new GroupMembersViewModel(_activeDirectory), group)
            {
                Owner = Application.Current.MainWindow
            };

            window.Show();
        }

        #endregion
    }
}
