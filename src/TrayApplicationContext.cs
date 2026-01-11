using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrayMaster.Models;

namespace TrayMaster
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly AppConfig _config;
        private readonly ProcessManager _processManager;
        private HttpListenerComponent? _httpListener;
        private LogViewer? _logViewer;
        private MenuItemConfig? _defaultMenuItem;
        private MenuItemConfig? _longRunningMenuItem;

        public TrayApplicationContext()
        {
            try
            {
                _config = ConfigManager.Load();
                _processManager = new ProcessManager();
                _processManager.StateChanged += OnProcessStateChanged;

                _trayIcon = new NotifyIcon
                {
                    Icon = IconManager.CreateIcon(_config.Icon),
                    Text = _config.TrayTitle,
                    Visible = true
                };

                _trayIcon.MouseClick += OnTrayIconClick;

                BuildMenu();

                // Initialize HTTP server if enabled
                if (_config.HttpServer?.Enabled == true)
                {
                    InitializeHttpServer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Application.Exit();
            }
        }

        private void InitializeHttpServer()
        {
            try
            {
                _httpListener = new HttpListenerComponent(
                    _config.HttpServer!,
                    _config.Menu.Items,
                    ExecuteMenuItemAsync
                );
                _httpListener.Start();

                // Update tray tooltip to indicate HTTP server is running
                var port = _config.HttpServer!.Port;
                var host = _config.HttpServer.LocalOnly ? "localhost" : "0.0.0.0";
                _trayIcon.Text = $"{_config.Name} - HTTP Server on {host}:{port}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start HTTP server:\n{ex.Message}\n\n" +
                    $"The application will continue without HTTP support.",
                    "HTTP Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private Task<bool> ExecuteMenuItemAsync(MenuItemConfig config)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Marshal to UI thread
            if (_trayIcon.ContextMenuStrip?.InvokeRequired == true)
            {
                _trayIcon.ContextMenuStrip.BeginInvoke(() =>
                {
                    try
                    {
                        ExecuteMenuItem(config);
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
            }
            else
            {
                try
                {
                    ExecuteMenuItem(config);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            return tcs.Task;
        }

        private void BuildMenu()
        {
            var contextMenu = new ContextMenuStrip();

            foreach (var item in _config.Menu.Items)
            {
                // Skip web-only items
                if (item.WebOnly)
                    continue;

                if (item.Label == "separator")
                {
                    contextMenu.Items.Add(new ToolStripSeparator());
                    continue;
                }

                // Track default and long-running items
                if (item.Default)
                    _defaultMenuItem = item;

                if (item.LongRunning)
                    _longRunningMenuItem = item;

                var menuItem = CreateMenuItem(item);
                contextMenu.Items.Add(menuItem);
            }

            _trayIcon.ContextMenuStrip = contextMenu;
            UpdateMenuVisibility();
        }

        private ToolStripMenuItem CreateMenuItem(MenuItemConfig config)
        {
            var menuItem = new ToolStripMenuItem();

            // Dynamic label for long-running processes
            if (config.LongRunning)
            {
                UpdateLongRunningMenuItem(menuItem, config);
            }
            else
            {
                menuItem.Text = config.Label;
                menuItem.Click += (s, e) => ExecuteMenuItem(config);
            }

            menuItem.Tag = config; // Store config for later reference
            return menuItem;
        }

        private void UpdateLongRunningMenuItem(ToolStripMenuItem menuItem, MenuItemConfig config)
        {
            menuItem.DropDownItems.Clear();

            if (_processManager.IsRunning)
            {
                // Show as submenu with View/Exit
                menuItem.Text = config.Label;
                menuItem.Click -= OnLongRunningMenuClick; // Remove click handler
                menuItem.Enabled = true; // Keep enabled so submenu works
                menuItem.DropDownItems.Add("View", null, (s, e) => ShowLogViewer());
                menuItem.DropDownItems.Add("Exit", null, (s, e) => _processManager.Stop());
            }
            else
            {
                // Show as single "Start" item
                menuItem.Text = $"Start {config.Label}";
                menuItem.Click -= OnLongRunningMenuClick; // Remove old handler
                menuItem.Click += OnLongRunningMenuClick; // Add new handler
                menuItem.Enabled = true;
            }
        }

        private void OnLongRunningMenuClick(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is MenuItemConfig config)
            {
                ExecuteMenuItem(config);
            }
        }

        private void ExecuteMenuItem(MenuItemConfig config)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(config.Action))
                {
                    switch (config.Action.ToLower())
                    {
                        case "quit":
                            QuitApplication();
                            return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(config.Command))
                {
                    var commandToExecute = config.Command;

                    // Build command with arguments for HTTP-triggered items
                    if (!string.IsNullOrWhiteSpace(config.Path))
                    {
                        // For HTTP-triggered items, always pass the request payload as JSON
                        var payload = _httpListener?.GetLastPayload();
                        if (payload != null &&
                            payload.Path.Equals(config.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            // Serialize full request payload to JSON
                            var jsonOptions = new JsonSerializerOptions
                            {
                                WriteIndented = false
                            };
                            var jsonPayload = JsonSerializer.Serialize(payload, jsonOptions);
                            
                            // Escape JSON for command line (replace " with \" and \ with \\)
                            var escapedPayload = jsonPayload
                                .Replace("\\", "\\\\")
                                .Replace("\"", "\\\"");
                            
                            // Add full request data as single JSON argument
                            commandToExecute += $" \"{escapedPayload}\"";
                        }
                        else
                        {
                            // If no payload found (e.g., triggered from tray menu), create minimal payload
                            var minimalPayload = new HttpPayload
                            {
                                Method = "GET",
                                Path = config.Path,
                                Query = new Dictionary<string, string>(),
                                Headers = new Dictionary<string, string>(),
                                Body = "",
                                ContentType = "",
                                RemoteIp = "127.0.0.1",
                                Url = $"http://127.0.0.1:{_config.HttpServer?.Port ?? 8080}{config.Path}",
                                ReceivedAt = DateTime.Now
                            };
                            
                            var jsonOptions = new JsonSerializerOptions
                            {
                                WriteIndented = false
                            };
                            var jsonPayload = JsonSerializer.Serialize(minimalPayload, jsonOptions);
                            
                            var escapedPayload = jsonPayload
                                .Replace("\\", "\\\\")
                                .Replace("\"", "\\\"");
                            
                            commandToExecute += $" \"{escapedPayload}\"";
                        }
                    }

                    if (config.LongRunning)
                    {
                        _processManager.Start(commandToExecute, config.WorkingDir);
                    }
                    else
                    {
                        ExecuteCommand(commandToExecute, config.WorkingDir);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error executing '{config.Label}':\n{ex.Message}",
                    "Execution Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ExecuteCommand(string command, string? workingDir)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!string.IsNullOrWhiteSpace(workingDir))
            {
                startInfo.WorkingDirectory = workingDir;
            }

            Process.Start(startInfo);
        }

        private void ShowLogViewer()
        {
            if (_logViewer == null || _logViewer.IsDisposed)
            {
                _logViewer = new LogViewer(_processManager, _config.Name);
            }

            _logViewer.Show();
            _logViewer.BringToFront();
        }

        private void OnTrayIconClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _defaultMenuItem != null)
            {
                // If it's a long-running process and already running, show log viewer instead
                if (_defaultMenuItem.LongRunning && _processManager.IsRunning)
                {
                    ShowLogViewer();
                }
                else
                {
                    ExecuteMenuItem(_defaultMenuItem);
                }
            }
        }

        private void OnProcessStateChanged(object? sender, ProcessState newState)
        {
            UpdateMenuVisibility();
            UpdateTrayTitle();
        }

        private void UpdateMenuVisibility()
        {
            if (_trayIcon.ContextMenuStrip == null)
                return;

            foreach (ToolStripItem item in _trayIcon.ContextMenuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Tag is MenuItemConfig config)
                {
                    // Update long-running menu items
                    if (config.LongRunning)
                    {
                        UpdateLongRunningMenuItem(menuItem, config);
                    }

                    // Handle visibility
                    menuItem.Visible = config.ShowWhen.ToLower() switch
                    {
                        "running" => _processManager.IsRunning,
                        "stopped" => !_processManager.IsRunning,
                        _ => true
                    };

                    // Handle enabled state
                    menuItem.Enabled = config.EnabledWhen.ToLower() switch
                    {
                        "running" => _processManager.IsRunning,
                        "stopped" => !_processManager.IsRunning,
                        _ => true
                    };
                }
            }
        }

        private void UpdateTrayTitle()
        {
            var status = _processManager.IsRunning ? "Running" : "Stopped";
            _trayIcon.Text = $"{_config.Name} - {status}";
        }

        private void QuitApplication()
        {
            // Stop HTTP server
            _httpListener?.Stop();

            // Stop any running process
            if (_processManager.IsRunning)
            {
                _processManager.Stop();
            }

            // Hide tray icon
            _trayIcon.Visible = false;

            // Exit the application
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _processManager.StateChanged -= OnProcessStateChanged;
                _processManager?.Dispose();
                _httpListener?.Dispose();
                _logViewer?.Dispose();
                _trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
