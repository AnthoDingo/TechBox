using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechBox.Models.Hardware;
using TechBox.ViewModels.Windows;
using TechBox.Views.Windows;

namespace TechBox.Controls
{
    /// <summary>
    /// Interaction logic for PowerCard.xaml
    /// </summary>
    public partial class PowerCard : UserControl, INotifyPropertyChanged
    {
        private const int RefreshIntervalMs = 10_000;

        private const int TickIntervalMs = 1_000;

        public static readonly DependencyProperty ComputerNameProperty = DependencyProperty.Register(nameof(ComputerName), typeof(string), typeof(PowerCard), new PropertyMetadata(default(string)));

        public event PropertyChangedEventHandler? PropertyChanged;

        private Computer _computer = new Computer();
        private Timer? _uptimeTimer;
        private Timer? _tickTimer;

        /// <summary>
        /// Set when a refresh fails, so the local tick stops passing off a stale uptime as live
        /// until the machine answers again.
        /// </summary>
        /// <remarks>Volatile: written by the refresh timer, read by the tick timer.</remarks>
        private volatile bool _isFaulted;

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

            // On a ThreadPool thread like the timer ticks below: GetUptime is an async method that
            // queries WMI synchronously, so awaiting it here would block the UI thread instead.
            await Task.Run(RefreshUptimeAsync);

            // Ticks on a ThreadPool thread separate from the UI thread: each tick re-queries WMI
            // and refreshes the uptime shown on this card every 10 seconds.
            _uptimeTimer = new Timer(async _ => await RefreshUptimeAsync(), null, RefreshIntervalMs, RefreshIntervalMs);

            // Between two of those, the displayed uptime keeps running on its own, every second.
            // Nothing is queried: the model derives the uptime from the boot time, a fixed point in
            // time, so simply reading it again gives the elapsed value. Each WMI refresh silently
            // resets the boot time, and the count carries on from whatever the machine reports.
            _tickTimer = new Timer(_ => Tick(), null, TickIntervalMs, TickIntervalMs);
        }

        private void Tick()
        {
            if (_isFaulted || _computer.LastBootUpTime is null)
            {
                return;
            }

            UptimeText = _computer.UptimeAsHuman;
        }

        private async Task RefreshUptimeAsync()
        {
            try
            {
                await _computer.GetUptime();
                UptimeText = _computer.UptimeAsHuman;
                _isFaulted = false;
            }
            catch (Exception ex)
            {
                _isFaulted = true;
                UptimeText = $"Erreur : {ex.Message}";
            }
        }

        public void StopTracking()
        {
            _uptimeTimer?.Dispose();
            _uptimeTimer = null;

            _tickTimer?.Dispose();
            _tickTimer = null;
        }

        public event Action<PowerCard>? RemoveEvent;

        [RelayCommand]
        public void Close()
        {
            StopTracking();
            RemoveEvent?.Invoke(this);
        }

        private void PowerIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PowerActionWindow window = new PowerActionWindow(new PowerActionViewModel(ComputerName))
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }
    }
}
