namespace TechBox.Services.Contracts
{
    /// <summary>
    /// Opens an application page in its own top-level window, detached from the main window's
    /// navigation.
    /// </summary>
    public interface IPageWindowService
    {
        /// <summary>
        /// Creates a new instance of the same page as <paramref name="sourcePage"/>, in a dedicated
        /// dependency injection scope, and shows it in a new window. The scope - and therefore the
        /// page's view model and every other scoped service it depends on - lives as long as the
        /// window.
        /// </summary>
        /// <param name="sourcePage">
        /// Page currently displayed in the main window. Its type is the page to build, and its view
        /// model is what the new one copies its state from when it implements
        /// <see cref="PluginContract.IExternalWindowState"/>.
        /// </param>
        /// <param name="title">Title of the new window. Defaults to the page type name when null or empty.</param>
        void OpenInNewWindow(object sourcePage, string? title = null);
    }
}
