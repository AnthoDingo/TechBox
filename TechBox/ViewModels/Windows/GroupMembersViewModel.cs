using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TechBox.Models.ActiveDirectory;
using TechBox.Services.Contracts;

namespace TechBox.ViewModels.Windows
{
    public partial class GroupMembersViewModel : ObservableObject
    {
        private readonly IActiveDirectory _activeDirectory;

        public GroupMembersViewModel(IActiveDirectory activeDirectory)
        {
            _activeDirectory = activeDirectory;
        }

        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private IEnumerable<GroupMember> _members = new List<GroupMember>();

        public async Task LoadMembersAsync(Group group)
        {
            GroupName = group.Name;
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                Members = await Task.Run(() => _activeDirectory.GetGroupMembers(group.DistinguishedName));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Impossible de charger les membres du groupe. ({ex.Message})";
                Debug.WriteLine($"Error while loading members of {group.DistinguishedName} : {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
