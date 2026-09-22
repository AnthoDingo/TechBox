using System.Windows.Media;
using System.Windows.Media.Imaging;
using TechBox.PluginContract;
using Wpf.Ui.Controls;

namespace TechBox.Views.Windows
{
    /// <summary>
    /// Generic window hosting a single application page, used by
    /// <see cref="Services.Contracts.IPageWindowService"/> to detach a page from the main window's
    /// navigation. The window owns no navigation of its own: it displays the page it was built with
    /// and nothing else.
    /// </summary>
    public partial class PageWindow : FluentWindow
    {
        private readonly object _page;
        private readonly object? _sourcePage;

        public PageWindow(object page, string title, object? sourcePage = null)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _sourcePage = sourcePage;

            InitializeComponent();

            Title = title;

            if (page is System.Windows.Controls.Page navigablePage)
            {
                _ = PageFrame.Navigate(navigablePage);
            }
            else
            {
                PageFrame.Content = page;
            }

            Loaded += async (_, _) => await InitializePageAsync();

            Closed += async (_, _) =>
            {
                if (GetNavigationAware(_page) is { } aware)
                {
                    await aware.OnNavigatedFromAsync();
                }
            };
        }

        /// <summary>
        /// Logo shown in the title bar, taken from the main window so that a plugin's branding
        /// override applies to detached pages too.
        /// </summary>
        public ImageSource LogoSource =>
            (System.Windows.Application.Current.MainWindow as MainWindow)?.ViewModel.LogoSource
            ?? new BitmapImage(new Uri("pack://application:,,,/Assets/wpfui-icon-256.png"));

        private async Task InitializePageAsync()
        {
            // A Frame navigated to from code does not raise WPF-UI's navigation life cycle, so the
            // page's view model is notified by hand - most of them load their lists there.
            if (GetNavigationAware(_page) is { } aware)
            {
                await aware.OnNavigatedToAsync();
            }

            // Then, and only then, the page catches up with what the main window is displaying: the
            // view model is a fresh instance, so without this the detached page would open empty.
            if (_sourcePage is not null
                && GetViewModel(_page) is IExternalWindowState state
                && GetViewModel(_sourcePage) is { } sourceViewModel)
            {
                await state.CopyStateFromAsync(sourceViewModel);
            }
        }

        /// <summary>
        /// Returns the page itself, or the view model it exposes through
        /// <see cref="INavigableView{T}"/>, when it takes part in the navigation life cycle.
        /// </summary>
        private static INavigationAware? GetNavigationAware(object page) =>
            page as INavigationAware ?? GetViewModel(page) as INavigationAware;

        /// <summary>View model a page exposes through <see cref="INavigableView{T}"/>, if any.</summary>
        private static object? GetViewModel(object page)
        {
            Type? navigableView = page
                .GetType()
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INavigableView<>));

            return navigableView?.GetProperty("ViewModel")?.GetValue(page);
        }
    }
}
