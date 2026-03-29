using RoboSharp;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace TechBox.Controls
{
    /// <summary>
    /// Interaction logic for FolderCard.xaml
    /// </summary>
    public partial class FolderCard : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty _title = DependencyProperty.Register(nameof(Title), typeof(string), typeof(FolderCard), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty _checked = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(FolderCard), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty _folderName = DependencyProperty.Register(nameof(FolderName), typeof(string), typeof(FolderCard), new PropertyMetadata(default(string)));

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _copyTask;
        private RoboCommand _roboCommand;

        private string _source;
        private string _destination;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FolderCard()
        {
            DataContext = this;
            InitializeComponent();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Title
        {
            get { return (string)GetValue(_title); }
            set { SetValue(_title, value); }
        }

        public string FolderName
        {
            get { return (string)GetValue(_folderName); }
            set { SetValue(_folderName, value); }
        }

        private bool _isPrepared = false;
        public bool IsPrepared
        {
            get
            {
                return _isPrepared;
            }
            set
            {
                _isPrepared = value;
                OnPropertyChanged(nameof(IsCheckEnabled));
                OnPropertyChanged(nameof(IsProgressVisible));
                OnPropertyChanged(nameof(IsProgressIndeterminate));
            }
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
                OnPropertyChanged(nameof(IsCheckEnabled));
                OnPropertyChanged(nameof(IsProgressIndeterminate));

            }
        }

        #region CheckBox
        public bool IsChecked
        {
            get { return (bool)GetValue(_checked); }
            set { SetValue(_checked, value); }
        }
        
        public bool IsCheckEnabled
        {
            get
            {
                if(_isPrepared || _isRunning)
                {
                    return false;
                } else
                {
                    return true;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
        }

        #endregion

        #region Progression

        public Visibility IsProgressVisible
        {
            get
            {
                if(IsPrepared || IsRunning)
                {
                    if(IsChecked)
                    {
                        return Visibility.Visible;
                    } else
                    {
                        return Visibility.Hidden;
                    }
                    
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
        }

        public bool IsProgressIndeterminate
        {
            get
            {
                if (IsPrepared && !IsRunning)
                {
                    return true;
                }
                {
                    return false;
                }
            }
        }

        private long _totaleFiles = 0;
        private long TotalFiles
        {
            get
            {
                return _totaleFiles;
            }
            set
            {
                _totaleFiles = value;
                OnPropertyChanged(nameof(TotalFiles));
                OnPropertyChanged(nameof(PercentComplete));
            }
        }

        private long _processedFiles = 0;
        private long ProcessedFiles
        {
            get
            {
                return _processedFiles;
            }
            set
            {
                _processedFiles = value;
                OnPropertyChanged(nameof(ProcessedFiles));
                OnPropertyChanged(nameof(PercentComplete));
            }
        }

        public int PercentComplete
        {
            get
            {
                if (TotalFiles == 0)
                    return 0;

                double percentage = (double)ProcessedFiles / TotalFiles * 100;
                Debug.WriteLine(percentage, "Percent");

                int roundPercentage = (int)Math.Round(percentage);
                Debug.WriteLine(roundPercentage, "Percent");
                return roundPercentage;
            }
        }

        #endregion

        public void PrepareCopy(string source, string destination)
        {

            IsPrepared = true;

            if (IsChecked == false)
                return;

            _source = $@"{source}\{FolderName}";
            _destination = $@"{destination}\{FolderName}";
                        
            // Code to run in Task
            _copyTask = new Task(() =>
            {
                IsRunning = true;

                OnPropertyChanged(nameof(IsProgressIndeterminate));
                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                if (!System.IO.Directory.Exists(_source))
                {
                    TotalFiles = 0;
                    return;
                }

                _roboCommand = new RoboCommand();

                _roboCommand.CopyOptions = new CopyOptions()
                {
                    Source = _source,
                    Destination = _destination,
                    Mirror = true,
                    CopySubdirectories = true,
                };
                _roboCommand.LoggingOptions = new LoggingOptions()
                {
                    VerboseOutput = true,
                };

                TotalFiles = System.IO.Directory.GetFiles(_source, "*.*", System.IO.SearchOption.AllDirectories).Length;
                _roboCommand.OnFileProcessed += (sender, args) =>
                {
                    // Incrémenter le compteur de fichiers
                    ProcessedFiles++;
                };

                _roboCommand.Start().Wait();

            }, _cts.Token);

            // When Task ended
            _copyTask.ContinueWith(_ =>
            {
                OnPropertyChanged(nameof(IsProgressVisible));
                ProcessedFiles = TotalFiles;
                IsRunning = false;
            });

            OnPropertyChanged(nameof(IsProgressVisible));
        }

        public async Task RunCopy()
        {
            if (_copyTask == null)
                return;

            // Start the Task
            _copyTask.Start();
            OnPropertyChanged(nameof(IsProgressVisible));
            _copyTask.Wait();
        }

        public void Cancel()
        {
            if(_copyTask != null && _copyTask.Status == TaskStatus.Running)
            {
                _cts.Cancel();
                _roboCommand.Stop();
                IsRunning = false;

                TotalFiles = 0;
                ProcessedFiles = 0;
            }
        }
        
        public void Release()
        {
            if (_copyTask != null)
                _copyTask = null;

            OnPropertyChanged(nameof(IsProgressVisible));
            IsPrepared = false;
        }

        public void Dispose()
        {
            if (_copyTask != null && _copyTask.Status == TaskStatus.Running)
            {
                _cts.Cancel();
                _roboCommand.Stop();
                IsRunning = false;
                IsPrepared = false;
            }
        }
    }
}
