using System.Windows.Media;
using System.Windows.Media.Imaging;
using TechBox.ViewModels.Windows;
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

        public PageWindow(object page, string title)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));

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

            // A Frame navigated to from code does not raise WPF-UI's navigation life cycle, so the
            // page's view model is notified by hand - most of them load their data there.
            Loaded += async (_, _) =>
            {
                if (GetNavigationAware(_page) is { } aware)
                {
                    await aware.OnNavigatedToAsync();
                }
            };

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

        /// <summary>
        /// Returns the page itself, or the view model it exposes through
        /// <see cref="INavigableView{T}"/>, when it takes part in the navigation life cycle.
        /// </summary>
        private static INavigationAware? GetNavigationAware(object page)
        {
            if (page is INavigationAware pageAware)
            {
                return pageAware;
            }

            Type? navigableView = page
                .GetType()
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INavigableView<>));

            return navigableView?.GetProperty("ViewModel")?.GetValue(page) as INavigationAware;
        }
    }
}
