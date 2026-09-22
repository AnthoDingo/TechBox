using Microsoft.Extensions.DependencyInjection;
using TechBox.Services.Contracts;
using TechBox.Views.Windows;

namespace TechBox.Services
{
    /// <inheritdoc cref="IPageWindowService"/>
    public sealed class PageWindowService : IPageWindowService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PageWindowService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void OpenInNewWindow(object sourcePage, string? title = null)
        {
            ArgumentNullException.ThrowIfNull(sourcePage);

            Type pageType = sourcePage.GetType();
            IServiceScope scope = _serviceScopeFactory.CreateScope();

            try
            {
                // ActivatorUtilities instead of a plain resolve: it always builds a *brand new* page,
                // and takes the page's own dependencies from this window's scope. A page and a view
                // model registered as scoped therefore get one independent instance per window, while
                // a page a plugin still registers as a singleton at least gets its own visual
                // instance - the same element cannot be displayed in two windows at once.
                object page = ActivatorUtilities.CreateInstance(scope.ServiceProvider, pageType);

                PageWindow window = new(page, string.IsNullOrWhiteSpace(title) ? pageType.Name : title!, sourcePage);
                window.Closed += (_, _) => scope.Dispose();
                window.Show();
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }
    }
}
