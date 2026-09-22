using TechBox.Models.ActiveDirectory;
using TechBox.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace TechBox.Views.Windows
{
    /// <summary>
    /// Non-modal window showing the members of an Active Directory group.
    /// </summary>
    public partial class GroupMembersWindow : FluentWindow
    {
        public GroupMembersViewModel ViewModel { get; }

        public GroupMembersWindow(GroupMembersViewModel viewModel, Group group)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Loaded += async (_, _) => await ViewModel.LoadMembersAsync(group);
        }
    }
}
