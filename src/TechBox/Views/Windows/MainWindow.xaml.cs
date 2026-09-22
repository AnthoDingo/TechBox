// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using TechBox.PluginContract;
using TechBox.Services;
using TechBox.Services.Contracts;
using TechBox.ViewModels.Windows;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace TechBox.Views.Windows
{
    public partial class MainWindow : FluentWindow, INavigationWindow
    {
        /// <summary>
        /// Dependency injection scope the navigated pages of this window are resolved from. Pages and
        /// their view models are registered as scoped, so this window keeps one instance of each -
        /// exactly like the previous singleton registrations - while a page opened in its own window
        /// gets a separate scope, and therefore a separate instance.
        /// </summary>
        private readonly IServiceScope _pageScope;

        private readonly IPageWindowService _pageWindowService;

        private object? _currentPage;

        /// <summary>Menu entry of the displayed page, when it allows being opened in its own window.</summary>
        private TechBoxNavigationViewItem? _currentExternalWindowItem;

        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            IServiceScopeFactory serviceScopeFactory,
            IPageWindowService pageWindowService,
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService
        )
        {

            ViewModel = viewModel;
            _pageWindowService = pageWindowService;
            _pageScope = serviceScopeFactory.CreateScope();
            DataContext = this;

            InitializeComponent();

            SetPageService(new ScopedNavigationViewPageProvider(_pageScope.ServiceProvider));

            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            contentDialogService.SetDialogHost(RootContentDialog);

            Loaded += async (_, _) => await ViewModel.Loaded();

            ViewModel.UpdateWindowDimension += (width, height) =>
            {
                this.Width = width;
                this.Height = height;

                // WindowStartupLocation only centers the window once, on its first Show(). Resizing it
                // afterwards (splash screen -> full content) leaves Left/Top untouched, so the window
                // has to be recentered on the work area by hand each time its dimensions change.
                this.Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - width) / 2;
                this.Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - height) / 2;
            };
        }

        #region INavigationWindow methods
        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _pageScope.Dispose();

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        #region New window

        /// <summary>
        /// The button is opt-in, per menu entry: it only appears for pages whose navigation item is a
        /// <see cref="TechBoxNavigationViewItem"/> with <c>AllowExternalWindow = true</c>. Every other
        /// page - and any navigation not coming from the menu - keeps it hidden.
        /// </summary>
        private void RootNavigation_OnNavigated(NavigationView sender, NavigatedEventArgs args)
        {
            _currentPage = args.Page;

            // Matched on the page type: RootNavigation.SelectedItem still holds the entry being
            // navigated away from at this point, which made the button lag one navigation behind.
            _currentExternalWindowItem = _currentPage is null
                ? null
                : ViewModel.FindExternalWindowItem(_currentPage.GetType());

            OpenInNewWindowButton.Visibility =
                _currentExternalWindowItem is not null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenInNewWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentPage is null || _currentExternalWindowItem is null)
            {
                return;
            }

            string? header = _currentExternalWindowItem.Content?.ToString();
            string title = string.IsNullOrWhiteSpace(header)
                ? ViewModel.ApplicationTitle
                : $"{ViewModel.ApplicationTitle} - {header}";

            _pageWindowService.OpenInNewWindow(_currentPage, title);
        }

        #endregion New window

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }


        #region Pane Mgmt

        private bool _isUserClosedPane;
        private bool _isPaneOpenedOrClosedFromCode;


        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isUserClosedPane)
            {
                return;
            }

            _isPaneOpenedOrClosedFromCode = true;
            RootNavigation.IsPaneOpen = !(e.NewSize.Width <= 1200);
            _isPaneOpenedOrClosedFromCode = false;
        }

        private void NavigationView_OnPaneOpened(NavigationView sender, RoutedEventArgs args)
        {
            if (_isPaneOpenedOrClosedFromCode)
            {
                return;
            }

            _isUserClosedPane = false;
        }

        private void NavigationView_OnPaneClosed(NavigationView sender, RoutedEventArgs args)
        {
            if (_isPaneOpenedOrClosedFromCode)
            {
                return;
            }

            _isUserClosedPane = true;
        }

        #endregion
    }
}
