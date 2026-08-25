namespace TechBox.ViewModels.Windows
{
    public partial class PowerActionViewModel : ObservableObject
    {
        public const string ActionShutdown = "Shutdown";
        public const string ActionReboot = "Reboot";

        public const string TimingNow = "Now";
        public const string TimingRemaining = "Time remaining";
        public const string TimingSchedule = "Schedule";

        [ObservableProperty]
        private string _computerName;

        [ObservableProperty]
        private IEnumerable<string> _actions = new List<string> { ActionShutdown, ActionReboot };

        [ObservableProperty]
        private string _selectedAction = ActionShutdown;

        [ObservableProperty]
        private IEnumerable<string> _timings = new List<string> { TimingNow, TimingRemaining, TimingSchedule };

        [ObservableProperty]
        private string _selectedTiming = TimingNow;

        private int _minutesRemaining = 1;
        public int MinutesRemaining
        {
            get => _minutesRemaining;
            set => SetProperty(ref _minutesRemaining, Math.Clamp(value, 1, 30));
        }

        [ObservableProperty]
        private DateTime? _scheduledDate = DateTime.Now.Date;

        [ObservableProperty]
        private string _scheduledTimeText = DateTime.Now.ToString("HH:mm");

        /// <summary>Earliest date selectable in the schedule DatePicker: today.</summary>
        public DateTime MinimumScheduledDate { get; } = DateTime.Now.Date;

        public PowerActionViewModel(string computerName)
        {
            ComputerName = computerName;
        }
    }
}
