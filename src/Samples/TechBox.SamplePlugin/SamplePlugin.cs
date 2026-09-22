using Microsoft.Extensions.DependencyInjection;
using TechBox.PluginContract;
using Wpf.Ui.Controls;

namespace TechBox.SamplePlugin
{
    /// <summary>
    /// Minimal plugin used as a template for plugin authors: build this project and drop the
    /// resulting <c>TechBox.SamplePlugin.dll</c> (with its dependencies) in TechBox's "Plugins"
    /// folder to see its page appear in the navigation menu.
    /// </summary>
    public sealed class SamplePlugin : ITechBoxPlugin
    {
        public string Name => "Exemple";

        public void ConfigureServices(IServiceCollection services)
        {
            // Scoped, not singleton: TechBox resolves pages from one dependency injection scope per
            // hosting window, so a scoped page gets its own instance (and its own view model state)
            // when the user opens it in a new window.
            services.AddScoped<SamplePage>();
            services.AddScoped<SampleViewModel>();
        }

        public IEnumerable<NavigationViewItem> CreateMenuItems()
        {
            // TechBoxNavigationViewItem rather than NavigationViewItem: it carries TechBox's own
            // per-page options. AllowExternalWindow shows the button that opens the page in its own
            // window - hidden by default, and on a plain NavigationViewItem.
            yield return new TechBoxNavigationViewItem
            {
                Content = Name,
                Icon = new SymbolIcon { Symbol = SymbolRegular.PuzzlePiece20 },
                TargetPageType = typeof(SamplePage),
                AllowExternalWindow = true,
            };
        }
    }
}
