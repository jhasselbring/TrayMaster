# TrayMaster - System Tray Process Manager

A lightweight Windows system tray application for managing long-running processes with JSON configuration and HTTP webhook support.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Code Signing](https://img.shields.io/badge/Code%20Signing-SignPath%20Foundation-blue)](https://signpath.io)

## Features

- 🎯 **System Tray Integration** - Manage processes from your Windows system tray
- 🔄 **Process Management** - Start, stop, and monitor long-running processes
- 🌐 **HTTP Webhooks** - Trigger menu items via HTTP requests for automation
- ⚙️ **JSON Configuration** - Simple, flexible configuration with all options documented
- 📝 **Log Viewer** - View process output in real-time
- 🎨 **Custom Icons** - Use .ico files or generate icons from text
- 🔧 **Working Directory** - Execute commands in specific directories
- 🚦 **Conditional Menus** - Show/enable menu items based on process state

## Quick Start

1. Download or build `TrayMaster.exe`
2. Copy `static/TrayMasterConfig.json.template` to your deployment directory as `TrayMasterConfig.json`
3. Add `icon.ico` (optional, or use the sample from `static/icon.ico`)
4. Configure your processes in `TrayMasterConfig.json`
5. Run `TrayMaster.exe`

## Configuration

### Basic Example

Create a `TrayMasterConfig.json` file:

```json
{
    "name": "ComfyUI",
    "trayTitle": "ComfyUI - Click to start",
    "icon": {
        "path": "icon.ico"
    },
    "menu": {
        "items": [
            {
                "label": "ComfyUI",
                "command": "node start_comfyui.js",
                "workingDir": "D:/Applications/ComfyUI",
                "longRunning": true,
                "default": true
            },
            {
                "label": "separator"
            },
            {
                "label": "ComfyUI Dir",
                "command": "explorer D:/Applications/ComfyUI"
            },
            {
                "label": "Quit",
                "action": "quit"
            }
        ]
    }
}
```

### Complete Configuration Reference

For a comprehensive example with **all available options** and inline documentation, see:
- **`static/TrayMasterConfig.json.template`** - Complete configuration template with all options explained

This template includes examples of:
- Icon configuration (file-based and text-generated)
- HTTP server settings
- Logging configuration
- All menu item options (longRunning, default, enabledWhen, showWhen, path, webOnly, etc.)
- Working directories
- Conditional menu items
- HTTP webhook endpoints

### Documentation & Examples

- **Configuration Template**: `static/TrayMasterConfig.json.template` - All options with inline comments
- **HTTP Webhook Guide**: `examples/HTTP-REFERENCE.md` - Complete HTTP webhook documentation
- **Example Handlers**: `examples/` - Node.js and Python webhook handler examples
- **Sample Icon**: `static/icon.ico` - Ready-to-use tray icon

## Menu Item Options

### Required Fields
- `label`: Text displayed in menu

### Optional Fields
- `command`: Command to execute
- `action`: Special action (`quit`)
- `longRunning`: Track process and show View/Exit submenu (default: `false`)
- `workingDir`: Working directory for command
- `default`: Execute on left-click of tray icon (default: `false`)
- `enabledWhen`: `"always"`, `"running"`, `"stopped"` (default: `"always"`)
- `showWhen`: `"always"`, `"running"`, `"stopped"` (default: `"always"`)
- `path`: HTTP endpoint path (e.g., `"/webhook"`) - makes item HTTP-accessible
- `webOnly`: Hide from tray menu, only accessible via HTTP (default: `false`)

### Long-Running Process Behavior

**When stopped:**
```
▶ Start ComfyUI    ← Click to start
```

**When running:**
```
▶ ComfyUI          ← Becomes submenu
  ├─ View          ← Show logs
  └─ Exit          ← Stop process
```

**Regular commands** (not long-running) always stay as single items.

## Icon Configuration

**Recommended:** Use a `.ico` file for best color accuracy:
```json
"icon": {
    "path": "icon.ico"
}
```

**Alternative:** Generate from text (simple colored square with letter):
```json
"icon": {
    "text": "C",
    "color": [73, 109, 137],
    "textColor": [255, 255, 255]
}
```

**Note:** PNG files are supported but .ico files preserve colors better in the system tray.

---

## HTTP Server (Webhook Support)

TrayMaster includes an optional HTTP server that allows menu items to be triggered via HTTP requests - perfect for webhooks, automation, and serverless-style workflows.

### Enable HTTP Server

Add the `httpServer` section to your `TrayMasterConfig.json`:

```json
{
    "name": "My App",
    "httpServer": {
        "enabled": true,
        "port": 8080,
        "localOnly": true
    },
    "menu": {
        "items": [...]
    }
}
```

**Configuration Options:**
- `enabled` (boolean): Enable/disable HTTP server (default: `false`)
- `port` (number): Port to listen on (default: `8080`, range: 1-65535)
- `localOnly` (boolean): Bind to localhost only (default: `true`)
  - `true`: Only accessible from `127.0.0.1` (more secure)
  - `false`: Accessible from any network interface

### HTTP-Enabled Menu Items

Add the `path` property to make menu items HTTP-accessible:

```json
{
    "label": "Process Webhook",
    "command": "node webhook-handler.js",
    "path": "/webhook"
}
```

**Triggers:**
- **From tray menu**: Click "Process Webhook"
- **From HTTP**: `curl http://127.0.0.1:8080/webhook`

### Web-Only Items (Hidden from Tray)

Use `webOnly: true` to create HTTP-only endpoints that don't appear in the tray menu:

```json
{
    "label": "Deploy Handler",
    "command": "node deploy.js",
    "path": "/deploy",
    "webOnly": true
}
```

This item is **only** accessible via HTTP, not from the tray menu.

### How Arguments Are Passed

When an HTTP request triggers a menu item, TrayMaster passes a single JSON argument containing all request data (ExpressJS-like format):

**Single Argument:** Complete request data as JSON string

The JSON object contains:
- `method` - HTTP method (GET, POST, PUT, PATCH, DELETE, etc.)
- `path` - URL path
- `query` - Query parameters object
- `headers` - Request headers object
- `body` - Request body string
- `contentType` - Content-Type header value
- `remoteIp` - Client IP address
- `url` - Full request URL
- `receivedAt` - Timestamp

**Example:**
- **GET request**: `node handler.js "{\"method\":\"GET\",\"path\":\"/webhook\",\"query\":{},\"headers\":{...},\"body\":\"\",...}"`
- **POST request**: `node handler.js "{\"method\":\"POST\",\"path\":\"/webhook\",\"query\":{},\"headers\":{...},\"body\":\"{\\\"user\\\":\\\"john\\\",\\\"action\\\":\\\"deploy\\\"}\",...}"`

### Node.js Handler Example

Create `handler.js`:

```javascript
const args = process.argv.slice(2);
const jsonRequestData = args[0];

// Parse the request data (ExpressJS-like format)
const req = JSON.parse(jsonRequestData);

console.log(`${req.method} ${req.path}`);

// Access query parameters (ExpressJS req.query)
const env = req.query.env || 'production';
console.log('Environment:', env);

// Access headers (ExpressJS req.headers)
const apiKey = req.headers['X-API-Key'] || req.headers['x-api-key'];
if (apiKey) {
    console.log('API Key provided');
}

// Access body (ExpressJS req.body)
if (req.body && req.contentType && req.contentType.includes('application/json')) {
    const data = JSON.parse(req.body);
    console.log('Payload:', data);

    // Process the webhook
    if (data.action === 'deploy') {
        console.log(`Deploying application to ${env}...`);
        // Your deployment logic here
    }
} else {
    console.log('No JSON body (GET request or non-JSON body)');
}
```

**Configuration:**
```json
{
    "label": "Webhook Handler",
    "command": "node handler.js",
    "path": "/webhook",
    "webOnly": true
}
```

**Test:**
```bash
# GET request with query parameters
curl "http://127.0.0.1:8080/webhook?env=production"

# POST request with JSON body
curl -X POST http://127.0.0.1:8080/webhook?env=production \
  -H "Content-Type: application/json" \
  -H "X-API-Key: secret123" \
  -d '{"action":"deploy","version":"1.0.0"}'
```

### Python Handler Example

Create `handler.py`:

```python
import sys
import json

args = sys.argv[1:]
json_request_data = args[0] if len(args) > 0 else None

if not json_request_data:
    print('Error: No request data provided', file=sys.stderr)
    sys.exit(1)

# Parse the request data (ExpressJS-like format)
req = json.loads(json_request_data)

print(f'{req.get("method")} {req.get("path")}')

# Access query parameters (ExpressJS req.query)
query = req.get('query', {})
env = query.get('env', 'production')
print(f'Environment: {env}')

# Access headers (ExpressJS req.headers)
headers = req.get('headers', {})
api_key = headers.get('X-API-Key') or headers.get('x-api-key')
if api_key:
    print('API Key provided')

# Access body (ExpressJS req.body)
body = req.get('body', '')
if body and 'application/json' in req.get('contentType', ''):
    data = json.loads(body)
    print(f'Payload: {data}')

    # Process the webhook
    if data.get('action') == 'deploy':
        print(f'Deploying application to {env}...')
        # Your deployment logic here
else:
    print('No JSON body (GET request or non-JSON body)')
```

**Configuration:**
```json
{
    "label": "Python Handler",
    "command": "python handler.py",
    "path": "/webhook",
    "webOnly": true
}
```

**Test:**
```bash
# GET request with query parameters
curl "http://127.0.0.1:8080/webhook?env=production"

# POST request with JSON body
curl -X POST http://127.0.0.1:8080/webhook?env=production \
  -H "Content-Type: application/json" \
  -H "X-API-Key: secret123" \
  -d '{"action":"deploy","version":"1.0.0"}'
```

### Endpoint Chaining (Advanced)

You can create endpoints that call other endpoints - useful for workflows and orchestration.

**Example: Endpoint that sends a POST to another endpoint**

Create `send-notification.js`:

```javascript
const http = require('http');
const args = process.argv.slice(2);

// Parse the incoming request data
const incomingReq = JSON.parse(args[0]);

const payload = {
    source: incomingReq.path,
    timestamp: new Date().toISOString(),
    message: 'Deployment complete',
    originalMethod: incomingReq.method,
    originalQuery: incomingReq.query
};

const options = {
    hostname: '127.0.0.1',
    port: 8080,
    path: '/notify',
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    }
};

const req = http.request(options);
req.write(JSON.stringify(payload));
req.end();
```

**Configuration:**
```json
{
    "menu": {
        "items": [
            {
                "label": "Deploy",
                "command": "node deploy.js",
                "path": "/deploy",
                "webOnly": true
            },
            {
                "label": "Send Notification",
                "command": "node send-notification.js",
                "path": "/deploy-notify",
                "webOnly": true
            },
            {
                "label": "Notification Handler",
                "command": "node notify-handler.js",
                "path": "/notify",
                "webOnly": true
            }
        ]
    }
}
```

Now `/deploy-notify` will trigger a chain: deploy-notify → /notify

### Complete HTTP Example

```json
{
    "name": "Webhook Server",
    "trayTitle": "Webhook Server - Running on :8080",
    "icon": {
        "text": "WH",
        "color": [41, 128, 185]
    },
    "httpServer": {
        "enabled": true,
        "port": 8080,
        "localOnly": true
    },
    "menu": {
        "items": [
            {
                "label": "GitHub Webhook",
                "command": "node github-handler.js",
                "path": "/github",
                "webOnly": true
            },
            {
                "label": "Deploy Handler",
                "command": "node deploy.js",
                "path": "/deploy",
                "webOnly": true
            },
            {
                "label": "Health Check",
                "command": "echo OK",
                "path": "/health",
                "webOnly": true
            },
            {
                "label": "separator"
            },
            {
                "label": "View Logs",
                "command": "notepad logs.txt"
            },
            {
                "label": "Quit",
                "action": "quit"
            }
        ]
    }
}
```

**Test endpoints:**
```bash
# Health check (GET)
curl http://127.0.0.1:8080/health

# Deploy (POST with payload)
curl -X POST http://127.0.0.1:8080/deploy -d '{"version":"1.0.0","env":"production"}'

# GitHub webhook (POST)
curl -X POST http://127.0.0.1:8080/github -d '{"action":"push","ref":"refs/heads/main"}'
```

### HTTP Response Codes

- **200 OK**: Request successfully executed
- **404 Not Found**: Path doesn't exist in configuration
- **500 Internal Server Error**: Command execution failed

### Security Considerations

1. **Use `localOnly: true`** (default) to prevent external access
2. **Authentication should be handled in endpoint handlers** (not in TrayMaster itself)
   - TrayMaster passes all headers to your handlers, so you can implement auth there
   - This keeps TrayMaster lightweight and follows serverless patterns
   - See "Authentication Patterns" section below for examples
3. **Alternative: Use a reverse proxy** (nginx, Caddy, etc.) for TrayMaster-level auth if needed
4. **Paths must be unique** - validation occurs at startup
5. **Commands run with TrayMaster's permissions** - be careful with sensitive operations

### Authentication Patterns

**Why handle auth in handlers (recommended):**

TrayMaster follows a **serverless-style architecture** where it acts as a lightweight router/gateway. Just like AWS Lambda, Azure Functions, or Vercel Functions:
- The gateway (Runner) forwards requests with full context
- The handler (your script) implements business logic AND authentication
- This keeps the gateway lightweight and flexible

**Example: API Key Authentication in Handler**

```javascript
// handler.js
const req = JSON.parse(process.argv[2]);

// Check API key from headers
const apiKey = req.headers['X-API-Key'] || req.headers['x-api-key'];
const validKey = process.env.API_KEY || 'your-secret-key';

if (apiKey !== validKey) {
    console.error('Unauthorized: Invalid API key');
    process.exit(1); // TrayMaster will return 500, or you can implement proper HTTP responses
}

// Proceed with authenticated request
console.log('Authenticated request received');
// ... your logic here
```

**Example: Basic Auth in Handler**

```javascript
// handler.js
const req = JSON.parse(process.argv[2]);

// Extract Basic Auth header
const authHeader = req.headers['Authorization'] || req.headers['authorization'];
if (!authHeader || !authHeader.startsWith('Basic ')) {
    console.error('Unauthorized: Missing Basic auth');
    process.exit(1);
}

const credentials = Buffer.from(authHeader.substring(6), 'base64').toString();
const [username, password] = credentials.split(':');

if (username !== 'admin' || password !== process.env.ADMIN_PASSWORD) {
    console.error('Unauthorized: Invalid credentials');
    process.exit(1);
}

// Authenticated - proceed
```

**Alternative: Reverse Proxy for TrayMaster-Level Auth**

If you need TrayMaster-level authentication (before requests reach handlers), use a reverse proxy:

```nginx
# nginx.conf
server {
    listen 80;
    server_name localhost;
    
    location / {
        # Basic auth at proxy level
        auth_basic "Restricted";
        auth_basic_user_file /path/to/.htpasswd;
        
        # Forward to TrayMaster
        proxy_pass http://127.0.0.1:8080;
    }
}
```

This approach:
- ✅ Keeps Runner lightweight
- ✅ Allows different auth per endpoint (via handler logic)
- ✅ Follows serverless patterns
- ✅ Easy to test and debug

### Tips

- **Debugging**: Use `start cmd.exe /k` prefix to keep terminal windows open:
  ```json
  "command": "start cmd.exe /k node handler.js"
  ```
- **Background processing**: Remove `start cmd.exe /k` to run silently
- **Working directory**: Use `workingDir` to set the execution directory
- **Long-running processes**: Not recommended for HTTP handlers (use `longRunning: false`)

---

## Building from Source

**Prerequisites:** .NET 8 SDK or later

**Quick build scripts:**
```powershell
.\build.ps1           # Debug build (quick iteration)
.\build-release.ps1   # Release build (single-file, optimized)
.\prepare-dist.ps1    # Prepare distribution folder
```

**Manual build:**
```bash
# Debug build
dotnet build

# Release build (single-file, ~69MB)
dotnet publish -c Release
```

**Output locations:**
- Debug: `bin\Debug\net8.0-windows\`
- Release: `bin\Release\net8.0-windows\win-x64\publish\`
- Distribution: `dist\` (created by `prepare-dist.ps1`)

**Distribution folder:**

The `prepare-dist.ps1` script creates a complete distribution package:
- Copies the release executable
- Copies configuration template from `static/TrayMasterConfig.json.template` → `dist/TrayMasterConfig.json`
- Copies sample icon from `static/icon.ico`
- Copies all example handlers from `examples/`
- Copies documentation (README.md, HTTP-REFERENCE.md)

**Requirements:** Windows 10 or later (self-contained, no .NET runtime needed)

## Example: Development Server

```json
{
    "name": "Dev Server",
    "trayTitle": "Dev Server - Click to start",
    "icon": {
        "text": "DEV",
        "color": [40, 167, 69]
    },
    "menu": {
        "items": [
            {
                "label": "Dev Server",
                "command": "npm run dev",
                "workingDir": "C:/Projects/MyApp",
                "longRunning": true,
                "default": true
            },
            {
                "label": "separator"
            },
            {
                "label": "Open Browser",
                "command": "start http://localhost:3000",
                "enabledWhen": "running"
            },
            {
                "label": "Quit",
                "action": "quit"
            }
        ]
    }
}
```

## Project Structure

**Root:**
- `build.ps1` - Debug build script
- `build-release.ps1` - Release build script
- `prepare-dist.ps1` - Prepare distribution folder with all files
- `TrayMaster.csproj` - Project file
- `README.md` - Main documentation (this file)
- `LICENSE` - MIT License
- `CODE-SIGNING-POLICY.md` - Code signing policy for SignPath Foundation

**`src/` - Source Code:**
- `Program.cs` - Entry point
- `TrayApplicationContext.cs` - Main tray logic and HTTP integration
- `HttpListenerComponent.cs` - HTTP server implementation
- `ProcessManager.cs` - Process lifecycle management
- `ConfigManager.cs` - Configuration loading and validation
- `IconManager.cs` - Icon handling
- `LogViewer.cs` - Log viewer window
- `Models/AppConfig.cs` - Data models (includes `HttpServerConfig`, `HttpPayload`)

**`static/` - Static Assets:**
- `icon.ico` - Sample tray icon
- `TrayMasterConfig.json.template` - Configuration template with all available options

**`examples/` - Example Handlers:**
- `handler.js` - Node.js webhook handler example
- `handler.py` - Python webhook handler example
- `send-post.js` - Node.js POST request sender (endpoint chaining)
- `send-post.py` - Python POST request sender (endpoint chaining)
- `view-request.js` - Example that displays the full HTTP request data
- `run-python.cmd` - Python wrapper for pyenv-win compatibility
- `README.md` - Examples documentation
- `HTTP-REFERENCE.md` - HTTP webhook reference guide

**`dist/` - Distribution Folder (created by `prepare-dist.ps1`):**
- `TrayMaster.exe` - The application (69MB, self-contained)
- `TrayMasterConfig.json` - Configuration file (copied from template)
- `icon.ico` - Custom tray icon
- All example handlers and documentation

---

## For Developers

**Project Organization:**
- **Source code** is in `src/` - organized for development
- **Static assets** are in `static/` - templates and samples
- **Examples** are in `examples/` - working code examples
- **Build scripts** are in root - for easy access
- **Development config** (`TrayMasterConfig.json`) is in root - for testing

**Making Changes:**
1. Modify source code in `src/`
2. Update `static/TrayMasterConfig.json.template` if adding new config options
3. Add examples to `examples/` if adding new features
4. Update documentation in `README.md`, `examples/HTTP-REFERENCE.md`, etc.
5. Run `.\build.ps1` to test changes
6. Run `.\build-release.ps1` then `.\prepare-dist.ps1` to create distribution

**Configuration Template:**

The `static/TrayMasterConfig.json.template` is the **single source of truth** for all available configuration options. It includes:
- Inline comments explaining every option
- Example values for all fields
- Guidance on when to use each option

When adding new configuration options, update this template first, then update the documentation.

---

## Roadmap

### Prioritized Features (In Order)

The following features are planned as top priorities:

1. **Static Server**: Serve static files from a configurable directory using the built-in HTTP server.
2. **HTTP Catch All**: Allow the HTTP server to handle any HTTP request, regardless of the request path, for maximum integration flexibility.
3. **Configuration Hot-Reload**: Automatically detect changes to `TrayMasterConfig.json` and reload configuration without requiring an application restart.
4. **Process Auto-Restart**: Ensure long-running processes are automatically restarted if they crash or exit unexpectedly, improving fault tolerance.
5. **Log File Persistence**: Save process output to persistent log files, with support for log rotation and file size limits to manage disk usage.
6. **Process Health Monitoring**: Display real-time CPU and memory usage of processes within the tray menu for easy monitoring.

### Backlog (Unordered)

- **HTTPS Support** - Add TLS/SSL support for HTTP server
- **Process Groups** - Group related processes together for batch start/stop operations
- **Environment Variables** - Support environment variable expansion in configuration
- **Notification System** - Show Windows notifications for process events (start, stop, crash)
- **Process Scheduling** - Schedule processes to start/stop at specific times
- **Remote Configuration** - Allow configuration updates via HTTP API
- **Process Limits** - Set resource limits (CPU, memory) for processes
- **Better Error Recovery** - Improved error handling and recovery mechanisms
- **Configuration Validation UI** - Visual feedback for configuration errors
- **Process Templates** - Reusable process templates for common patterns
- **Statistics Dashboard** - Track process uptime, restart counts, and usage statistics
- **Plugin System** - Extend functionality through plugins or scripts
- **Cross-Platform Support** - Port to Linux and macOS (requires significant refactoring)
- **Web UI** - Optional web-based interface for managing processes
- **Process Logging Levels** - Filter and control log verbosity per process

---

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License - Copyright (c) 2026 jhasselbring

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software.
```

## Code Signing

TrayMaster releases are code-signed using certificates provided by the **SignPath Foundation** for open source projects.

- **Certificate Authority**: SignPath Foundation
- **Code Signing Policy**: [CODE-SIGNING-POLICY.md](CODE-SIGNING-POLICY.md)
- **Setup Guide**: [SIGNING-SETUP.md](SIGNING-SETUP.md)

Code signing ensures:
- Verified publisher authenticity
- Binary integrity (no tampering)
- No Windows SmartScreen warnings
- User trust and security

Learn more: https://signpath.io

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

All contributors must:
- Enable multi-factor authentication (MFA) on GitHub
- Follow the code signing policy
- Respect the MIT License terms

## Security

To report security vulnerabilities:
1. Open an issue: https://github.com/jhasselbring/runner/issues
2. Include detailed reproduction steps
3. Allow time for investigation and patching

See [CODE-SIGNING-POLICY.md](CODE-SIGNING-POLICY.md) for our security commitments.

---

**Built with .NET 8** | **Open Source** | **MIT Licensed** | **Code Signed**
