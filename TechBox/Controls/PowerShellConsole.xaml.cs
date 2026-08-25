using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TechBox.Controls
{
    /// <summary>
    /// Console interactive hébergeant une session Windows PowerShell 5.1 (powershell.exe) via un
    /// processus dont les flux standard sont redirigés. Le composant démarre sa propre session à
    /// l'affichage et la termine lorsqu'il est déchargé.
    /// </summary>
    public partial class PowerShellConsole : UserControl
    {
        private const string PowerShellExecutable = "powershell.exe";

        private Process? _process;
        private readonly List<string> _history = new();
        private int _historyIndex;

        public PowerShellConsole()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                if (_process is null)
                {
                    StartSession();
                }
                InputBox.Focus();
            };
            Unloaded += (_, _) => StopSession();
        }

        private void StartSession()
        {
            OutputBox.Clear();
            _history.Clear();
            _historyIndex = 0;

            ProcessStartInfo startInfo = new(PowerShellExecutable)
            {
                Arguments = "-NoLogo -NoProfile -ExecutionPolicy Bypass",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            try
            {
                _process.Start();
            }
            catch (Exception ex)
            {
                AppendOutput($"Impossible de démarrer {PowerShellExecutable} : {ex.Message}\r\n");
                _process = null;
                return;
            }

            _ = PumpOutputAsync(_process.StandardOutput);
            _ = PumpOutputAsync(_process.StandardError);
        }

        private async Task PumpOutputAsync(StreamReader reader)
        {
            char[] buffer = new char[4096];
            try
            {
                int read;
                while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string text = new(buffer, 0, read);
                    await Dispatcher.InvokeAsync(() => AppendOutput(text));
                }
            }
            catch (ObjectDisposedException)
            {
                // La session a été arrêtée pendant la lecture : rien à faire.
            }
            catch (IOException)
            {
                // Le processus a été tué pendant la lecture : rien à faire.
            }
        }

        private void AppendOutput(string text)
        {
            OutputBox.AppendText(text);
            OutputBox.ScrollToEnd();
        }

        private void StopSession()
        {
            if (_process is null)
            {
                return;
            }

            Process process = _process;
            _process = null;

            try
            {
                if (!process.HasExited)
                {
                    process.StandardInput.WriteLine("exit");
                    process.StandardInput.Flush();
                    if (!process.WaitForExit(1500))
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
            }
            catch
            {
                // Le processus est déjà terminé ou inaccessible.
            }
            finally
            {
                process.Dispose();
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StopSession();
            StartSession();
            InputBox.Focus();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Clear();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    SubmitCommand();
                    e.Handled = true;
                    break;
                case Key.Up:
                    NavigateHistory(-1);
                    e.Handled = true;
                    break;
                case Key.Down:
                    NavigateHistory(1);
                    e.Handled = true;
                    break;
            }
        }

        private void SubmitCommand()
        {
            string command = InputBox.Text;
            InputBox.Clear();

            if (_process is null || _process.HasExited)
            {
                AppendOutput("La session PowerShell n'est plus active. Utilisez « Redémarrer ».\r\n");
                return;
            }

            if (!string.IsNullOrEmpty(command) && (_history.Count == 0 || _history[^1] != command))
            {
                _history.Add(command);
            }
            _historyIndex = _history.Count;

            try
            {
                _process.StandardInput.WriteLine(command);
                _process.StandardInput.Flush();
            }
            catch (Exception ex)
            {
                AppendOutput($"Erreur lors de l'envoi de la commande : {ex.Message}\r\n");
            }
        }

        private void NavigateHistory(int direction)
        {
            if (_history.Count == 0)
            {
                return;
            }

            _historyIndex = Math.Clamp(_historyIndex + direction, 0, _history.Count);
            InputBox.Text = _historyIndex < _history.Count ? _history[_historyIndex] : string.Empty;
            InputBox.CaretIndex = InputBox.Text.Length;
        }
    }
}
