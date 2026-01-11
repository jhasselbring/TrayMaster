using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Runner.Models;

namespace Runner
{
    public class HttpListenerComponent : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Dictionary<string, MenuItemConfig> _pathMapping;
        private readonly Func<MenuItemConfig, Task<bool>> _executeCallback;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _listenerTask;
        private readonly object _payloadLock = new object();
        private HttpPayload? _lastPayload;

        public HttpListenerComponent(
            HttpServerConfig config,
            IEnumerable<MenuItemConfig> menuItems,
            Func<MenuItemConfig, Task<bool>> executeCallback)
        {
            _executeCallback = executeCallback ?? throw new ArgumentNullException(nameof(executeCallback));
            _cancellationTokenSource = new CancellationTokenSource();

            // Build path mapping
            _pathMapping = menuItems
                .Where(item => !string.IsNullOrWhiteSpace(item.Path))
                .ToDictionary(item => item.Path!, StringComparer.OrdinalIgnoreCase);

            // Create and configure listener
            _listener = new HttpListener();
            var prefix = config.LocalOnly
                ? $"http://127.0.0.1:{config.Port}/"
                : $"http://+:{config.Port}/";
            _listener.Prefixes.Add(prefix);

            LogInfo($"HTTP server configured on {prefix}");
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                _listenerTask = Task.Run(ListenAsync, _cancellationTokenSource.Token);
                LogInfo("HTTP server started successfully");
            }
            catch (HttpListenerException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to start HTTP server. Port may be in use or requires admin privileges: {ex.Message}",
                    ex
                );
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
            _listenerTask?.Wait(TimeSpan.FromSeconds(5));
            LogInfo("HTTP server stopped");
        }

        public HttpPayload? GetLastPayload()
        {
            lock (_payloadLock)
            {
                return _lastPayload;
            }
        }

        private async Task ListenAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    // Handle request without awaiting (fire and forget with error handling)
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (HttpListenerException)
                {
                    // Expected when stopping the listener
                    break;
                }
                catch (Exception ex)
                {
                    LogError($"Listener error: {ex.Message}");
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                LogInfo($"Received {request.HttpMethod} request: {request.Url?.AbsolutePath}");

                var path = request.Url?.AbsolutePath ?? "/";

                // Find matching menu item
                if (!_pathMapping.TryGetValue(path, out var menuItem))
                {
                    await SendResponse(response, 404, $"Path not found: {path}");
                    return;
                }

                // Capture comprehensive request data
                var method = request.HttpMethod;
                var queryParams = ParseQueryString(request.QueryString);
                var headers = ParseHeaders(request.Headers);
                var body = await ReadRequestBodyAsync(request);
                var contentType = request.ContentType ?? "";
                var remoteIp = request.RemoteEndPoint?.Address?.ToString() ?? "";
                var fullUrl = request.Url?.ToString() ?? "";

                // Store payload with all captured data
                lock (_payloadLock)
                {
                    _lastPayload = new HttpPayload
                    {
                        Method = method,
                        Path = path,
                        Query = queryParams,
                        Headers = headers,
                        Body = body,
                        ContentType = contentType,
                        RemoteIp = remoteIp,
                        Url = fullUrl,
                        ReceivedAt = DateTime.Now
                    };
                }

                LogInfo($"Stored payload for {path}: {body.Length} bytes, method: {method}");

                // Execute menu item (this will marshal to UI thread)
                var success = await _executeCallback(menuItem);

                if (success)
                {
                    await SendResponse(response, 200, $"Successfully executed: {menuItem.Label}");
                }
                else
                {
                    await SendResponse(response, 500, $"Failed to execute: {menuItem.Label}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling request: {ex.Message}");
                await SendResponse(response, 500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
                return string.Empty;

            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            return await reader.ReadToEndAsync();
        }

        private Dictionary<string, string> ParseQueryString(System.Collections.Specialized.NameValueCollection queryString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (string key in queryString.AllKeys)
            {
                if (key != null)
                {
                    var value = queryString[key];
                    result[key] = value ?? "";
                }
            }
            
            return result;
        }

        private Dictionary<string, string> ParseHeaders(System.Collections.Specialized.NameValueCollection headers)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (string key in headers.AllKeys)
            {
                if (key != null)
                {
                    var value = headers[key];
                    result[key] = value ?? "";
                }
            }
            
            return result;
        }

        private async Task SendResponse(HttpListenerResponse response, int statusCode, string message)
        {
            try
            {
                response.StatusCode = statusCode;
                response.ContentType = "text/plain";
                var buffer = Encoding.UTF8.GetBytes(message);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                LogError($"Error sending response: {ex.Message}");
            }
        }

        private void LogInfo(string message)
        {
            Console.WriteLine($"[HTTP] {message}");
        }

        private void LogError(string message)
        {
            Console.WriteLine($"[HTTP ERROR] {message}");
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
            _listener?.Close();
        }
    }
}
