namespace TechBox.SamplePlugin
{
    /// <summary>Minimal view model backing <see cref="SamplePage"/>.</summary>
    public sealed class SampleViewModel
    {
        public int ClickCount { get; private set; }

        public void Increment() => ClickCount++;
    }
}
