using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using TechBox.Databases;
using TechBox.Models;
using TechBox.Models.Hardware;
using TechBox.Statics;
using Wpf.Ui.Controls;

namespace TechBox.Controls
{
    /// <summary>
    /// Interaction logic for CCMCard.xaml
    /// </summary>
    public partial class CCMCard : UserControl, INotifyPropertyChanged
	{

		public static readonly DependencyProperty ComputerNameProperty = DependencyProperty.Register(nameof(ComputerName), typeof(string), typeof(CCMCard), new PropertyMetadata(default(string)));

		public event PropertyChangedEventHandler? PropertyChanged;

		private IEnumerable<SCCMAction> actions = new List<SCCMAction>();
        CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken cancellationToken;

		public CCMCard()
        {
			DataContext = this;
            InitializeComponent();
			cancellationToken = cts.Token;
        }

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool _isRunning = false;
		private bool IsRunning
		{
			get
			{
				return _isRunning;
			}
			set
			{
				_isRunning = value;
				OnPropertyChanged(nameof(IsRunning));
				OnPropertyChanged(nameof(ButtonIcon));
			}
		}

		private bool _isCancel = false;

		public event Action<CCMCard> RemoveEvent;

	#region Display Variables
		public string ComputerName
		{
			get { return (string)GetValue(ComputerNameProperty); }
			set { SetValue(ComputerNameProperty, value); }
		}

		private bool _ping = true;
		public bool Ping
		{
			get
			{
				return _ping;
			}
			set
			{
				_ping = value;
				OnPropertyChanged(nameof(Ping));
                OnPropertyChanged(nameof(IsIndeterminate));
            }
		}

		private bool _isIndeterminate = true;
		public bool IsIndeterminate
		{
			get
			{
				//return string.IsNullOrEmpty(CurrentAction) ? true : false;
				return Ping;
			}
		}

		private int _percentComplete = 0;
		public int PercentComplete
		{
			get
			{
				return _percentComplete;
            }
			set
			{
				_percentComplete = value;
				OnPropertyChanged(nameof(PercentComplete));
				OnPropertyChanged(nameof(DisplayPercentComplete));
			}
		}
		public int DisplayPercentComplete
		{
			get
			{
				if (MaxPercent == 0)
					return 0;

                return (int)(_percentComplete * 100) / MaxPercent;
            }
		}

		private int _maxPercent = 0;
		public int MaxPercent
		{
			get
			{
				return _maxPercent;
			}
			set
			{
				_maxPercent = value;
				OnPropertyChanged(nameof(MaxPercent));
			}
		}

		private string _currentAction = string.Empty;
		public string CurrentAction
		{
			get
			{
				return _currentAction;
			}
			set
			{
				_currentAction = value;
				try
				{
                    OnPropertyChanged(nameof(CurrentAction));
                } catch(Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
				
			}
		}

		private SymbolRegular _symbolIcon = SymbolRegular.PlugConnected24;
		public SymbolRegular SymbolIcon
		{
			get
			{
				return _symbolIcon;
			}
			set
			{
				_symbolIcon = value;
                OnPropertyChanged(nameof(SymbolIcon));
                OnPropertyChanged(nameof(IconColor));
            }
		}
		public SolidColorBrush IconColor
		{
			get
			{
				if (SymbolIcon == SymbolRegular.PlugConnected24)
				{
					return new SolidColorBrush(Colors.Green);
                } else
				{
					return new SolidColorBrush(Colors.Red);
                }
			}
		}

		private SymbolRegular _buttonIcon = SymbolRegular.Stop16;
		public SymbolIcon ButtonIcon
		{
			get
			{
				if (_isRunning)
				{
					return new SymbolIcon(SymbolRegular.Stop16) { Filled = true};
                } else
				{
					return new SymbolIcon(SymbolRegular.Delete16) { Filled = true };
				}
			}
		}

		#endregion

		private int _errors = 0;

        public async Task RunActions()
		{
			await Task.Delay(2 * 1000, cancellationToken);

            Computer computer = new Computer() { Name = ComputerName };

            CurrentAction = "Checking Computer availability";
            if (!computer.IsOnline())
            {
                SymbolIcon = SymbolRegular.PlugDisconnected24;
                MaxPercent = 1;
                PercentComplete = 1;
                CurrentAction = "Computer Offline";
				IsRunning = false;
				Ping = false;
                return;
            }

			Ping = false;
            IsRunning = true;

            actions = (new SQLiteContext()).SCCMActions
                .Where(a => a.IsEnabled == true)
                .ToList();

            MaxPercent = actions.Count();

            Dictionary<string, object> methodArgs = new Dictionary<string, object>();
            foreach (SCCMAction action in actions)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    CurrentAction = $"Running {action.Name}";

                    methodArgs["sScheduleID"] = action.ClientAction;

                    bool result = Management.InvokeCCMAction(ComputerName, action.ClientAction);

                    PercentComplete += 1;

                    if (result)
					{
                        await Task.Delay(20 * 1000, cancellationToken);
                    } else
					{
						_errors += 1;
					}
                        
                } else
				{
					IsRunning = false;
					_isCancel = true;
					CurrentAction = "Canceled";
				}
            }

			if (!_isCancel)
				CurrentAction = "Complete";

			if (_errors > 0)
				CurrentAction = $"{CurrentAction} with errors";
            IsRunning = false;
        }

        [RelayCommand]
        public void Cancel()
		{
            cts.Cancel();
            if (IsRunning)
			{
                IsRunning = false;
				CurrentAction = "Canceled";
				if(_errors > 0)
                    CurrentAction = $"{CurrentAction} with errors";

                _isCancel = true;
            } else
			{
				OnRemoveEvent();
			}
		}

		protected virtual void OnRemoveEvent()
		{
			RemoveEvent?.Invoke(this);
		}
	}
}
