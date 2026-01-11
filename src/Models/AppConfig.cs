using System;
using System.Collections.Generic;

namespace TrayMaster.Models
{
    public class AppConfig
    {
        public string Name { get; set; } = "Runner";
        public string TrayTitle { get; set; } = "Runner";
        public IconConfig? Icon { get; set; }
        public MenuConfig Menu { get; set; } = new MenuConfig();
        public LoggingConfig? Logging { get; set; }
        public HttpServerConfig? HttpServer { get; set; }
    }

    public class IconConfig
    {
        public string? Path { get; set; }
        public int[]? Color { get; set; }
        public string? Text { get; set; }
        public int[]? TextColor { get; set; }
    }

    public class MenuConfig
    {
        public List<MenuItemConfig> Items { get; set; } = new List<MenuItemConfig>();
    }

    public class MenuItemConfig
    {
        public string Label { get; set; } = "";
        public string? Command { get; set; }
        public string? Action { get; set; }
        public string? WorkingDir { get; set; }
        public bool LongRunning { get; set; }
        public bool Default { get; set; }
        public string EnabledWhen { get; set; } = "always";
        public string ShowWhen { get; set; } = "always";
        public string? Path { get; set; }
        public bool WebOnly { get; set; }
    }

    public class LoggingConfig
    {
        public bool Enabled { get; set; } = true;
        public string Level { get; set; } = "info";
        public int MaxLines { get; set; } = 10000;
    }

    public class HttpServerConfig
    {
        public bool Enabled { get; set; } = false;
        public int Port { get; set; } = 8080;
        public bool LocalOnly { get; set; } = true;
    }

    public class HttpPayload
    {
        public string Method { get; set; } = "";
        public string Path { get; set; } = "";
        public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string RemoteIp { get; set; } = "";
        public string Url { get; set; } = "";
        public DateTime ReceivedAt { get; set; }
    }
}
