using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Runner.Models;

namespace Runner
{
    public class ConfigManager
    {
        private const string ConfigFileName = "runner.json";

        public static AppConfig Load()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    $"Configuration file not found: {configPath}\n\n" +
                    $"Please create a runner.json file in the same directory as Runner.exe"
                );
            }

            try
            {
                var json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                if (config == null)
                {
                    throw new InvalidOperationException("Failed to deserialize configuration");
                }

                Validate(config);
                return config;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Invalid JSON in config.json:\n{ex.Message}",
                    ex
                );
            }
        }

        private static void Validate(AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.Name))
            {
                throw new InvalidOperationException("Configuration 'name' is required");
            }

            if (config.Menu?.Items == null || config.Menu.Items.Count == 0)
            {
                throw new InvalidOperationException("Configuration must have at least one menu item");
            }

            foreach (var item in config.Menu.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Label))
                {
                    throw new InvalidOperationException("Menu item 'label' is required");
                }

                if (item.Label != "separator" &&
                    string.IsNullOrWhiteSpace(item.Command) &&
                    string.IsNullOrWhiteSpace(item.Action))
                {
                    throw new InvalidOperationException(
                        $"Menu item '{item.Label}' must have either 'command' or 'action'"
                    );
                }
            }

            // Validate HTTP server configuration
            if (config.HttpServer?.Enabled == true)
            {
                // Validate port range
                if (config.HttpServer.Port < 1 || config.HttpServer.Port > 65535)
                {
                    throw new InvalidOperationException(
                        $"HTTP port must be between 1 and 65535, got {config.HttpServer.Port}"
                    );
                }

                var pathItems = config.Menu.Items
                    .Where(item => !string.IsNullOrWhiteSpace(item.Path))
                    .ToList();

                // Validate HTTP paths are unique
                var duplicatePaths = pathItems
                    .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

                if (duplicatePaths.Any())
                {
                    throw new InvalidOperationException(
                        $"Duplicate HTTP paths found: {string.Join(", ", duplicatePaths)}"
                    );
                }

                // Validate paths start with /
                var invalidPaths = pathItems
                    .Where(item => !item.Path!.StartsWith("/"))
                    .Select(item => item.Path)
                    .ToList();

                if (invalidPaths.Any())
                {
                    throw new InvalidOperationException(
                        $"HTTP paths must start with '/': {string.Join(", ", invalidPaths)}"
                    );
                }
            }

            // Validate web-only items have paths
            var webOnlyWithoutPath = config.Menu.Items
                .Where(item => item.WebOnly && string.IsNullOrWhiteSpace(item.Path))
                .Select(item => item.Label)
                .ToList();

            if (webOnlyWithoutPath.Any())
            {
                throw new InvalidOperationException(
                    $"Web-only menu items must have a 'path': {string.Join(", ", webOnlyWithoutPath)}"
                );
            }
        }
    }
}
