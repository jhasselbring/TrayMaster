# HTTP Webhook Reference - Complete Guide

> **See Also:**
> - `../static/TrayMasterConfig.json.template` - Complete configuration with all options
> - `handler.js` / `handler.py` - Working examples in this folder
> - `../README.md` - Main Runner documentation

## How HTTP Requests Work in Runner

When an HTTP request hits an endpoint, Runner executes the configured command and passes data as **command-line arguments**.

---

## Arguments Passed to Your Script

### Single Argument: Complete Request Data (JSON)

Runner passes a single JSON string containing all request information in an ExpressJS-like format.

**The JSON object structure:**
```json
{
  "method": "GET|POST|PUT|PATCH|DELETE|etc",
  "path": "/api/deploy",
  "query": {
    "param1": "value1",
    "param2": "value2"
  },
  "headers": {
    "Content-Type": "application/json",
    "User-Agent": "curl/7.68.0",
    "X-Custom-Header": "value"
  },
  "body": "request body as string",
  "contentType": "application/json",
  "remoteIp": "127.0.0.1",
  "url": "http://127.0.0.1:8080/api/deploy?param1=value1",
  "receivedAt": "2024-01-15T10:30:00.1234567"
}
```

**Example:** For `curl -X POST http://127.0.0.1:8080/api/deploy?env=prod -d '{"version":"1.0.0"}' -H "X-Key: secret"`

Your script receives:
- **Argument 1** = Complete JSON string with method, path, query, headers, body, etc.

**All HTTP methods** (GET, POST, PUT, PATCH, DELETE, etc.) receive the same format. The `body` field will be empty for methods that don't typically include a body.

---

## Complete Example: Node.js Script

### Configuration (TrayMasterConfig.json)
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
                "label": "Deploy Handler",
                "command": "node deploy.js",
                "path": "/api/deploy",
                "webOnly": true,
                "workingDir": "C:/MyApp/scripts"
            }
        ]
    }
}
```

### Script (deploy.js)
```javascript
#!/usr/bin/env node

// Get command-line arguments
const args = process.argv.slice(2);
const jsonRequestData = args[0];  // Single argument: complete request data

// Parse the request data (ExpressJS-like format)
const req = JSON.parse(jsonRequestData);

console.log('=== Deploy Handler ===');
console.log('Method:', req.method);
console.log('Path:', req.path);
console.log('Remote IP:', req.remoteIp);

// Access query parameters (ExpressJS req.query)
const env = req.query.env || 'production';
console.log('Environment:', env);

// Access headers (ExpressJS req.headers)
const apiKey = req.headers['X-API-Key'] || req.headers['x-api-key'];
if (apiKey) {
    console.log('API Key provided');
}

// Access body (ExpressJS req.body)
if (req.body) {
    // Parse body if it's JSON
    if (req.contentType && req.contentType.includes('application/json')) {
        const data = JSON.parse(req.body);
        console.log('Payload:', data);

        // Access specific fields
        const version = data.version;
        console.log(`Deploying version ${version} to ${env}...`);

        // Your deployment logic here
        if (data.action === 'rollback') {
            console.log('Rolling back...');
        }
    } else {
        console.log('Body (non-JSON):', req.body);
    }
} else {
    console.log('No body (GET request)');
    console.log('Usage: POST with JSON body');
}
```

### HTTP Requests

**GET Request:**
```bash
curl "http://127.0.0.1:8080/api/deploy?env=staging"
```

**Execution:**
```bash
node deploy.js "{\"method\":\"GET\",\"path\":\"/api/deploy\",\"query\":{\"env\":\"staging\"},\"headers\":{...},\"body\":\"\",...}"
```

**POST Request:**
```bash
curl -X POST http://127.0.0.1:8080/api/deploy?env=staging \
  -H "Content-Type: application/json" \
  -H "X-API-Key: secret123" \
  -d '{"version":"1.0.0","action":"deploy"}'
```

**Execution:**
```bash
node deploy.js "{\"method\":\"POST\",\"path\":\"/api/deploy\",\"query\":{\"env\":\"staging\"},\"headers\":{\"Content-Type\":\"application/json\",\"X-API-Key\":\"secret123\"},\"body\":\"{\\\"version\\\":\\\"1.0.0\\\",\\\"action\\\":\\\"deploy\\\"}\",...}"
```

---

## Complete Example: Python Script

### Configuration (TrayMasterConfig.json)
```json
{
    "menu": {
        "items": [
            {
                "label": "Python Handler",
                "command": "python webhook.py",
                "path": "/webhook",
                "webOnly": true
            }
        ]
    }
}
```

### Script (webhook.py)
```python
#!/usr/bin/env python3
import sys
import json

# Get command-line arguments
args = sys.argv[1:]  # Skip script name
json_request_data = args[0] if len(args) > 0 else None

if not json_request_data:
    print('Error: No request data provided')
    sys.exit(1)

# Parse the request data (ExpressJS-like format)
req = json.loads(json_request_data)

print('=== Webhook Handler ===')
print(f'Method: {req.get("method")}')
print(f'Path: {req.get("path")}')
print(f'Remote IP: {req.get("remoteIp")}')

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
if body:
    # Parse body if it's JSON
    content_type = req.get('contentType', '')
    if 'application/json' in content_type:
        data = json.loads(body)
        print(f'Payload: {data}')

        # Access specific fields
        event = data.get('event')
        user = data.get('user', 'unknown')

        print(f'Processing {event} from {user}...')

        # Your webhook logic here
        if event == 'deployment':
            print('Deploying application...')
    else:
        print(f'Body (non-JSON): {body}')
else:
    # GET request
    print('GET request - no payload')
    print('Usage: POST with JSON body')
```

---

## Configuration Options Reference

### Required Fields

| Field | Type | Description |
|-------|------|-------------|
| `label` | string | Display name in tray menu (or internal name if webOnly) |
| Either `command` OR `action` | string | What to execute |

### Optional Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `command` | string | - | Shell command to execute (e.g., `"node script.js"`) |
| `action` | string | - | Built-in action (currently only `"quit"`) |
| `path` | string | - | HTTP endpoint path (must start with `/`) |
| `webOnly` | boolean | `false` | Hide from tray menu, HTTP-only |
| `workingDir` | string | - | Working directory for command execution |
| `longRunning` | boolean | `false` | Track process state, show start/stop controls |
| `default` | boolean | `false` | Execute on left-click of tray icon |
| `enabledWhen` | string | `"always"` | When item is enabled: `"always"`, `"running"`, `"stopped"` |
| `showWhen` | string | `"always"` | When item is visible: `"always"`, `"running"`, `"stopped"` |

### HTTP Server Options

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `enabled` | boolean | `false` | Enable HTTP server |
| `port` | number | `8080` | Port to listen on (1-65535) |
| `localOnly` | boolean | `true` | Bind to localhost only (more secure) |

---

## Common Patterns

### Pattern 1: API Endpoint for External Webhooks

**Use Case:** Receive webhooks from GitHub, GitLab, etc.

```json
{
    "label": "GitHub Webhook",
    "command": "node github-handler.js",
    "path": "/webhooks/github",
    "webOnly": true
}
```

**Test:**
```bash
curl -X POST http://127.0.0.1:8080/webhooks/github \
  -d '{"action":"push","ref":"refs/heads/main"}'
```

### Pattern 2: Deployment Trigger

**Use Case:** Trigger deployments via HTTP.

```json
{
    "label": "Deploy",
    "command": "node deploy.js",
    "path": "/deploy",
    "webOnly": true,
    "workingDir": "C:/Projects/MyApp"
}
```

**Test:**
```bash
curl -X POST http://127.0.0.1:8080/deploy \
  -d '{"version":"1.2.3","env":"production"}'
```

### Pattern 3: Health Check

**Use Case:** Simple health check endpoint.

```json
{
    "label": "Health Check",
    "command": "echo OK",
    "path": "/health",
    "webOnly": true
}
```

**Test:**
```bash
curl http://127.0.0.1:8080/health
```

### Pattern 4: Visible Terminal for Debugging

**Use Case:** Keep terminal open to see script output.

```json
{
    "label": "Debug Handler",
    "command": "start cmd.exe /k node debug.js",
    "path": "/debug",
    "webOnly": true
}
```

Terminal stays open after script completes.

### Pattern 5: Silent Background Processing

**Use Case:** Run script silently without showing terminal.

```json
{
    "label": "Background Job",
    "command": "node background.js",
    "path": "/background",
    "webOnly": true
}
```

No terminal window appears.

### Pattern 6: Both Tray and HTTP

**Use Case:** Trigger from both tray menu and HTTP.

```json
{
    "label": "Restart Service",
    "command": "node restart.js",
    "path": "/restart"
    // Note: webOnly is false (default), so appears in tray
}
```

Can be triggered:
- From tray: Click "Restart Service"
- From HTTP: `curl http://127.0.0.1:8080/restart`

---

## How to Structure Your Scripts

### Template: Node.js Handler

```javascript
const args = process.argv.slice(2);
const jsonRequestData = args[0];

if (!jsonRequestData) {
    console.error('Error: No request data provided');
    process.exit(1);
}

try {
    // Parse the request data (ExpressJS-like format)
    const req = JSON.parse(jsonRequestData);

    // Always log the request info
    console.log(`${req.method} ${req.path}`);

    // Access query parameters (ExpressJS req.query)
    const action = req.query.action;

    // Or access from body if it's a POST request
    let bodyData = null;
    if (req.body && req.contentType && req.contentType.includes('application/json')) {
        bodyData = JSON.parse(req.body);
    }

    // Route based on action (from query or body)
    const actionToProcess = action || bodyData?.action;

    if (!actionToProcess) {
        console.error('Error: action is required (query param or body field)');
        process.exit(1);
    }

    switch (actionToProcess) {
        case 'deploy':
            handleDeploy(req, bodyData);
            break;
        case 'rollback':
            handleRollback(req, bodyData);
            break;
        default:
            console.error('Unknown action:', actionToProcess);
            process.exit(1);
    }
} catch (error) {
    console.error('Error parsing request:', error.message);
    process.exit(1);
}

function handleDeploy(req, bodyData) {
    console.log('Deploying...', bodyData);
    // Access req.query, req.headers, req.body, etc.
    // Your deployment logic
}

function handleRollback(req, bodyData) {
    console.log('Rolling back...', bodyData);
    // Your rollback logic
}
```

### Template: Python Handler

```python
import sys
import json

args = sys.argv[1:]
json_request_data = args[0] if len(args) > 0 else None

if not json_request_data:
    print('Error: No request data provided', file=sys.stderr)
    sys.exit(1)

try:
    # Parse the request data (ExpressJS-like format)
    req = json.loads(json_request_data)

    # Always log the request info
    print(f'{req.get("method")} {req.get("path")}')

    # Access query parameters (ExpressJS req.query)
    query = req.get('query', {})
    action = query.get('action')

    # Or access from body if it's a POST request
    body_data = None
    body = req.get('body', '')
    if body and 'application/json' in req.get('contentType', ''):
        body_data = json.loads(body)

    # Route based on action (from query or body)
    action_to_process = action or (body_data.get('action') if body_data else None)

    if not action_to_process:
        print('Error: action is required (query param or body field)', file=sys.stderr)
        sys.exit(1)

    if action_to_process == 'deploy':
        handle_deploy(req, body_data)
    elif action_to_process == 'rollback':
        handle_rollback(req, body_data)
    else:
        print(f'Unknown action: {action_to_process}', file=sys.stderr)
        sys.exit(1)

except json.JSONDecodeError as e:
    print(f'Error parsing request: {e}', file=sys.stderr)
    sys.exit(1)

def handle_deploy(req, body_data):
    print('Deploying...', body_data)
    # Access req['query'], req['headers'], req['body'], etc.
    # Your deployment logic

def handle_rollback(req, body_data):
    print('Rolling back...', body_data)
    # Your rollback logic
```

---

## Error Handling

### Script Exit Codes
- **Exit 0**: Success (Runner returns HTTP 200)
- **Exit non-zero**: Failure (Runner returns HTTP 500)

### Example with Error Handling

```javascript
const args = process.argv.slice(2);
const jsonRequestData = args[0];

try {
    if (!jsonRequestData) {
        console.error('Error: No request data provided');
        process.exit(1);
    }

    const req = JSON.parse(jsonRequestData);

    // Validate method
    if (req.method !== 'POST') {
        console.error('Error: POST request required');
        process.exit(1);
    }

    // Parse body if present
    let data = null;
    if (req.body && req.contentType && req.contentType.includes('application/json')) {
        data = JSON.parse(req.body);
    }

    if (!data || !data.version) {
        console.error('Error: version is required in body');
        process.exit(1);
    }

    // Your logic
    console.log('Success!');
    process.exit(0);  // Success

} catch (error) {
    console.error('Error:', error.message);
    process.exit(1);  // Failure
}
```

---

## Validation Rules

### Path Validation
- Must start with `/`
- Must be unique across all menu items
- Case-insensitive

**Valid:**
- `/webhook`
- `/api/deploy`
- `/v1/events`

**Invalid:**
- `webhook` (missing `/`)
- `/webhook` (if already defined elsewhere)

### Web-Only Validation
If `webOnly: true`, must have a `path` defined.

**Valid:**
```json
{
    "label": "Handler",
    "command": "node handler.js",
    "path": "/handler",
    "webOnly": true
}
```

**Invalid:**
```json
{
    "label": "Handler",
    "command": "node handler.js",
    "webOnly": true
    // Error: web-only items must have a path
}
```

---

## Tips & Best Practices

1. **Use `webOnly: true` for webhooks** - Keeps tray menu clean
2. **Use `workingDir`** - Ensures scripts can find their dependencies
3. **Validate JSON in scripts** - Always check for required fields
4. **Use `start cmd.exe /k` for debugging** - Keeps terminal open
5. **Remove `start cmd.exe /k` for production** - Silent execution
6. **Log request path** - Helps with debugging
7. **Return proper exit codes** - 0 for success, non-zero for errors
8. **Keep scripts simple** - One script per endpoint is easier to maintain

---

## Full Working Example

**TrayMasterConfig.json:**
```json
{
    "name": "My API Server",
    "trayTitle": "API Server - Port 8080",
    "httpServer": {
        "enabled": true,
        "port": 8080,
        "localOnly": true
    },
    "menu": {
        "items": [
            {
                "label": "Deploy API",
                "command": "node deploy.js",
                "path": "/api/deploy",
                "webOnly": true,
                "workingDir": "C:/MyApp"
            },
            {
                "label": "separator"
            },
            {
                "label": "Quit",
                "action": "quit"
            }
        ]
    }
}
```

**deploy.js:**
```javascript
const args = process.argv.slice(2);
const jsonRequestData = args[0];

// Parse the request data (ExpressJS-like format)
const req = JSON.parse(jsonRequestData);

console.log(`${req.method} ${req.path}`);

// Access query parameters
const env = req.query.env || 'production';
console.log(`Environment: ${env}`);

// Access body
if (req.body && req.contentType && req.contentType.includes('application/json')) {
    const data = JSON.parse(req.body);
    console.log(`Deploying version ${data.version} to ${env}...`);
    // Deployment logic here
} else {
    console.log('Use: POST /api/deploy?env=prod {"version":"1.0.0"}');
}
```

**Test:**
```bash
curl -X POST http://127.0.0.1:8080/api/deploy \
  -d '{"version":"1.0.0"}'
```

---

See `runner.example.json` for a complete configuration reference with all options.
