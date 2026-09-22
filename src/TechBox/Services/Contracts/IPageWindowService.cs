namespace TechBox.Services.Contracts
{
    /// <summary>
    /// Opens an application page in its own top-level window, detached from the main window's
    /// navigation.
    /// </summary>
    public interface IPageWindowService
    {
        /// <summary>
        /// Creates a new instance of <paramref name="pageType"/> in a dedicated dependency injection
        /// scope and shows it in a new window. The scope - and therefore the page's view model and
        /// every other scoped service it depends on - lives as long as the window.
        /// </summary>
        /// <param name="pageType">Type of the page to host, typically a <see cref="System.Windows.Controls.Page"/>.</param>
        /// <param name="title">Title of the new window. Defaults to the page type name when null or empty.</param>
        void OpenInNewWindow(Type pageType, string? title = null);
    }
}
