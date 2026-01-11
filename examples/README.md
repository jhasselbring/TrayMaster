# HTTP Webhook Examples

This folder contains example handlers and complete documentation for TrayMaster's HTTP webhook functionality.

## Documentation Files

- **`HTTP-REFERENCE.md`** - **START HERE!** Complete guide on how HTTP webhooks work
  - How arguments are passed to scripts
  - All configuration options explained
  - Node.js and Python templates
  - Common patterns and best practices

- **`../static/TrayMasterConfig.json.template`** - Complete configuration template with all possible options
  - Inline comments explaining every field
  - Multiple real-world examples
  - All menu item options (longRunning, enabledWhen, showWhen, path, webOnly, etc.)
  - HTTP server configuration
  - Icon and logging configuration

## Example Scripts

- **`handler.js`** - Node.js webhook handler (demonstrates parsing request data)
- **`handler.py`** - Python webhook handler (demonstrates parsing request data)
- **`send-post.js`** - Node.js HTTP client for endpoint chaining
- **`send-post.py`** - Python HTTP client for endpoint chaining
- **`view-request.js`** - Display full HTTP request data for debugging
- **`run-python.cmd`** - Python wrapper for pyenv-win compatibility

## Quick Start

**1. Read the documentation:**
```
examples/HTTP-REFERENCE.md       ← Complete HTTP webhook guide
static/TrayMasterConfig.json.template      ← All configuration options
```

**2. Set up your deployment directory:**
```bash
# Copy the configuration template
cp static/TrayMasterConfig.json.template ./TrayMasterConfig.json

# Copy example handlers (optional)
cp examples/*.js .
cp examples/*.py .
cp examples/run-python.cmd .

# Copy the icon (optional)
cp static/icon.ico .
```

**3. Configure your TrayMasterConfig.json:**
```json
{
    "httpServer": {
        "enabled": true,
        "port": 8080,
        "localOnly": true
    },
    "menu": {
        "items": [
            {
                "label": "Node Handler",
                "command": "start cmd.exe /k node handler.js",
                "path": "/node",
                "webOnly": true
            }
        ]
    }
}
```

**4. Test:**
```bash
curl -X POST http://127.0.0.1:8080/node -d '{"action":"test"}'
```

## How It Works

When you trigger an HTTP endpoint, Runner passes a single JSON argument containing all request data (ExpressJS-like format):

**Single Argument:** Complete request data as JSON string

The JSON object contains:
- `method` - HTTP method (GET, POST, etc.)
- `path` - URL path
- `query` - Query parameters object
- `headers` - Request headers object
- `body` - Request body string
- `contentType` - Content-Type header value
- `remoteIp` - Client IP address
- `url` - Full request URL
- `receivedAt` - Timestamp

**Example execution:**
```bash
# Your configuration
"command": "node deploy.js"
"path": "/api/deploy"

# HTTP request
curl -X POST http://127.0.0.1:8080/api/deploy?env=prod -d '{"version":"1.0.0"}' -H "X-Custom-Header: value"

# How Runner calls your script (single JSON argument)
node deploy.js "{\"method\":\"POST\",\"path\":\"/api/deploy\",\"query\":{\"env\":\"prod\"},\"headers\":{\"X-Custom-Header\":\"value\"},\"body\":\"{\\\"version\\\":\\\"1.0.0\\\"}\",...}"
```

See `HTTP-REFERENCE.md` for complete details!
