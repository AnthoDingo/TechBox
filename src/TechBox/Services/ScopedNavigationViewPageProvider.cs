using Wpf.Ui.Abstractions;

namespace TechBox.Services
{
    /// <summary>
    /// <see cref="INavigationViewPageProvider"/> resolving pages from a given dependency injection
    /// scope instead of the root provider, so that pages registered with
    /// <c>AddScoped</c> get one instance per hosting window.
    /// </summary>
    public sealed class ScopedNavigationViewPageProvider : INavigationViewPageProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ScopedNavigationViewPageProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetPage(Type pageType) => _serviceProvider.GetService(pageType)!;
    }
}
