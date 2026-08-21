// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using TechBox.Databases;
using TechBox.Services;
using TechBox.Services.Contracts;
using TechBox.ViewModels.Pages;
using TechBox.ViewModels.Windows;
using TechBox.Views.Pages;
using TechBox.Views.Windows;
using Wpf.Ui.DependencyInjection;

namespace TechBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { 
                c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
                c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                c.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();
                services.AddHostedService<ApplicationHostService>();

                services.AddDbContext<SQLiteContext>();

                services
                    .AddSingleton<INavigationWindow, MainWindow>()                
                    .AddSingleton<MainWindowViewModel>()                    
                    .AddSingleton<INavigationService, NavigationService>()
                    .AddSingleton<ISnackbarService, SnackbarService>()
                    .AddSingleton<IContentDialogService, ContentDialogService>()
                    .AddSingleton<IActiveDirectory, ActiveDirectoryService>()
                    ;

                services
                    .AddTransient<AdExplorerWindow>()
                    .AddTransient<AdExplorerViewModel>()
                    .AddTransient<GroupMembersWindow>()
                    .AddTransient<GroupMembersViewModel>()
                    ;

                services
                    .AddSingleton<HomePage>()
                    .AddSingleton<HomeViewModel>()
                    .AddSingleton<DashboardPage>()
                    .AddSingleton<DashboardViewModel>()
                    ;
                    //.AddSingleton<IvantiPage>()
                    //.AddSingleton<IvantiViewModel>();

                // Users Pages
                services
                    .AddSingleton<Views.Pages.Users.InfoPage>()
                    .AddSingleton<ViewModels.Pages.Users.InfoViewModel>();

                // Computers Pages
                services
                    .AddSingleton<Views.Pages.Computers.InfoPage>()
                    .AddSingleton<ViewModels.Pages.Computers.InfoViewModel>()
                    .AddSingleton<Views.Pages.Computers.SCCMPage>()
                    .AddSingleton<ViewModels.Pages.Computers.SCCMViewModel>()
                    .AddSingleton<Views.Pages.Computers.BackupProfilePage>()
                    .AddSingleton<ViewModels.Pages.Computers.BackupProfileViewModel>()
                    ;
                //services.AddSingleton<ViewModels.Pages.Computers.RestoreProfileViewModel>();
                //services.AddSingleton<Views.Pages.Computers.RestoreProfilePage>();

                // Tools Pages
                services
                    .AddSingleton<Views.Pages.Tools.ActiveDirectoryPage>()
                    .AddSingleton<ViewModels.Pages.Tools.ActiveDirectoryViewModel>()
                    ;

                services
                    .AddSingleton<DataPage>()
                    .AddSingleton<DataViewModel>()                    
                    .AddSingleton<SettingsPage>()
                    .AddSingleton<SettingsViewModel>()
                    ;
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
