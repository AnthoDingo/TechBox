using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TechBox.Models.Hardware;

namespace TechBox.Controls
{
    /// <summary>
    /// Interaction logic for PowerCard.xaml
    /// </summary>
    public partial class PowerCard : UserControl, INotifyPropertyChanged
    {
        private const int RefreshIntervalMs = 10_000;

        public static readonly DependencyProperty ComputerNameProperty = DependencyProperty.Register(nameof(ComputerName), typeof(string), typeof(PowerCard), new PropertyMetadata(default(string)));

        public event PropertyChangedEventHandler? PropertyChanged;

        private Computer _computer = new Computer();
        private Timer? _uptimeTimer;

        public PowerCard()
        {
            DataContext = this;
            InitializeComponent();
            Unloaded += (_, _) => StopTracking();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ComputerName
        {
            get { return (string)GetValue(ComputerNameProperty); }
            set { SetValue(ComputerNameProperty, value); }
        }

        private string _uptimeText = "Interrogation en cours...";
        public string UptimeText
        {
            get { return _uptimeText; }
            private set
            {
                _uptimeText = value;
                OnPropertyChanged(nameof(UptimeText));
            }
        }

        public async Task StartTracking()
        {
            _computer = new Computer { Name = ComputerName };

            if (!await Task.Run(() => _computer.IsOnline()))
            {
                UptimeText = "Poste injoignable.";
                return;
            }

            await RefreshUptimeAsync();

            // Ticks on a ThreadPool thread separate from the UI thread: each tick re-queries WMI
            // and refreshes the uptime shown on this card every 10 seconds.
            _uptimeTimer = new Timer(async _ => await RefreshUptimeAsync(), null, RefreshIntervalMs, RefreshIntervalMs);
        }

        private async Task RefreshUptimeAsync()
        {
            try
            {
                await _computer.GetUptime();
                UptimeText = _computer.UptimeAsHuman;
            }
            catch (Exception ex)
            {
                UptimeText = $"Erreur : {ex.Message}";
            }
        }

        public void StopTracking()
        {
            _uptimeTimer?.Dispose();
            _uptimeTimer = null;
        }

        public event Action<PowerCard>? RemoveEvent;

        [RelayCommand]
        public void Close()
        {
            StopTracking();
            RemoveEvent?.Invoke(this);
        }
    }
}
