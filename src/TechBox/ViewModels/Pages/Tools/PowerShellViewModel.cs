using System.ComponentModel;

namespace TechBox.ViewModels.Pages.Tools
{
    public partial class PowerShellViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
    {
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        public Task OnNavigatedToAsync() => Task.CompletedTask;
    }
}
