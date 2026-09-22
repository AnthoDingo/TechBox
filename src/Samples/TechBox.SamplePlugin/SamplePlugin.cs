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
            services.AddSingleton<SamplePage>();
            services.AddSingleton<SampleViewModel>();
        }

        public IEnumerable<NavigationViewItem> CreateMenuItems()
        {
            yield return new NavigationViewItem
            {
                Content = Name,
                Icon = new SymbolIcon { Symbol = SymbolRegular.PuzzlePiece20 },
                TargetPageType = typeof(SamplePage),
            };
        }
    }
}
