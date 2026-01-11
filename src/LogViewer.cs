using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrayMaster
{
    public class LogViewer : Form
    {
        private readonly RichTextBox _logTextBox;
        private readonly ProcessManager _processManager;

        public LogViewer(ProcessManager processManager, string appName)
        {
            _processManager = processManager;

            Text = $"{appName} - Logs";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;

            _logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None
            };

            Controls.Add(_logTextBox);

            // Subscribe to process output
            _processManager.OutputReceived += OnOutputReceived;

            // Load existing output
            var existingOutput = _processManager.GetOutput();
            if (!string.IsNullOrEmpty(existingOutput))
            {
                _logTextBox.Text = existingOutput;
                ScrollToBottom();
            }

            // Cleanup on close
            FormClosing += (s, e) =>
            {
                e.Cancel = true; // Don't actually close, just hide
                Hide();
            };
        }

        private void OnOutputReceived(object? sender, string line)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnOutputReceived(sender, line));
                return;
            }

            AppendLine(line);
        }

        private void AppendLine(string line)
        {
            _logTextBox.SelectionStart = _logTextBox.TextLength;

            // Color errors in red
            if (line.Contains("[ERROR]"))
            {
                _logTextBox.SelectionColor = Color.FromArgb(255, 100, 100);
            }
            else
            {
                _logTextBox.SelectionColor = Color.FromArgb(220, 220, 220);
            }

            _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} {line}\n");
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.ScrollToCaret();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _processManager.OutputReceived -= OnOutputReceived;
            }
            base.Dispose(disposing);
        }
    }
}
