using System;
using System.Diagnostics;
using System.Text;

namespace TrayMaster
{
    public enum ProcessState
    {
        Stopped,
        Starting,
        Running,
        Stopping
    }

    public class ProcessManager : IDisposable
    {
        private Process? _process;
        private readonly StringBuilder _outputBuffer = new StringBuilder();
        private readonly object _lock = new object();

        public ProcessState State { get; private set; } = ProcessState.Stopped;
        public event EventHandler<ProcessState>? StateChanged;
        public event EventHandler<string>? OutputReceived;

        public bool IsRunning => State == ProcessState.Running;

        public void Start(string command, string? workingDirectory = null)
        {
            lock (_lock)
            {
                if (_process != null && !_process.HasExited)
                {
                    throw new InvalidOperationException("Process is already running");
                }

                ChangeState(ProcessState.Starting);

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                if (!string.IsNullOrWhiteSpace(workingDirectory))
                {
                    startInfo.WorkingDirectory = workingDirectory;
                }

                _process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                _process.OutputDataReceived += OnOutputReceived;
                _process.ErrorDataReceived += OnErrorReceived;
                _process.Exited += OnProcessExited;

                try
                {
                    _process.Start();
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    ChangeState(ProcessState.Running);
                }
                catch (Exception ex)
                {
                    ChangeState(ProcessState.Stopped);
                    throw new InvalidOperationException($"Failed to start process: {ex.Message}", ex);
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_process == null || _process.HasExited)
                {
                    ChangeState(ProcessState.Stopped);
                    return;
                }

                ChangeState(ProcessState.Stopping);

                try
                {
                    // Try graceful shutdown first
                    _process.CloseMainWindow();

                    if (!_process.WaitForExit(3000))
                    {
                        // Force kill if needed
                        _process.Kill(entireProcessTree: true);
                        _process.WaitForExit(1000);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping process: {ex.Message}");
                }
                finally
                {
                    _process?.Dispose();
                    _process = null;
                    ChangeState(ProcessState.Stopped);
                }
            }
        }

        public string GetOutput()
        {
            lock (_lock)
            {
                return _outputBuffer.ToString();
            }
        }

        private void OnOutputReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                lock (_lock)
                {
                    _outputBuffer.AppendLine(e.Data);
                }
                OutputReceived?.Invoke(this, e.Data);
            }
        }

        private void OnErrorReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                lock (_lock)
                {
                    _outputBuffer.AppendLine($"[ERROR] {e.Data}");
                }
                OutputReceived?.Invoke(this, $"[ERROR] {e.Data}");
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            ChangeState(ProcessState.Stopped);
        }

        private void ChangeState(ProcessState newState)
        {
            if (State != newState)
            {
                State = newState;
                StateChanged?.Invoke(this, newState);
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
