#!/usr/bin/env node

// Node.js HTTP Request Handler
// Usage: node handler.js <json_request_data>
//
// The request data is a JSON object containing:
// - method: HTTP method (GET, POST, etc.)
// - path: URL path
// - query: Query parameters object
// - headers: Request headers object
// - body: Request body string
// - contentType: Content-Type header value
// - remoteIp: Client IP address
// - url: Full request URL
// - receivedAt: Timestamp

const args = process.argv.slice(2);
const jsonRequestData = args[0];

console.log('=================================');
console.log('  Node.js HTTP Request Handler');
console.log('=================================');
console.log();

if (!jsonRequestData) {
    console.log('No request data provided');
    console.log('=================================');
    process.exit(1);
}

// Check if we received the old format (just a path string) vs new format (JSON)
if (jsonRequestData.startsWith('/') && !jsonRequestData.startsWith('{')) {
    console.error('ERROR: Received old format (path string) instead of JSON.');
    console.error('This usually means the Runner app needs to be rebuilt.');
    console.error('Received:', jsonRequestData);
    console.error('');
    console.error('Expected format: JSON object with method, path, query, headers, body, etc.');
    console.log('=================================');
    process.exit(1);
}

try {
    // Parse the request data (ExpressJS-like format)
    const req = JSON.parse(jsonRequestData);

    console.log('=== REQUEST INFORMATION ===');
    console.log(`Method: ${req.method || 'N/A'}`);
    console.log(`Path: ${req.path || 'N/A'}`);
    console.log(`URL: ${req.url || 'N/A'}`);
    console.log(`Remote IP: ${req.remoteIp || 'N/A'}`);
    console.log(`Content-Type: ${req.contentType || 'N/A'}`);
    console.log(`Received At: ${req.receivedAt || 'N/A'}`);
    console.log();

    // Query parameters (ExpressJS req.query)
    if (req.query && Object.keys(req.query).length > 0) {
        console.log('=== QUERY PARAMETERS ===');
        for (const [key, value] of Object.entries(req.query)) {
            console.log(`  ${key}: ${value}`);
        }
        console.log();
    }

    // Headers (ExpressJS req.headers)
    if (req.headers && Object.keys(req.headers).length > 0) {
        console.log('=== REQUEST HEADERS ===');
        for (const [key, value] of Object.entries(req.headers)) {
            console.log(`  ${key}: ${value}`);
        }
        console.log();
    }

    // Body (ExpressJS req.body)
    if (req.body) {
        console.log('=== REQUEST BODY ===');
        console.log('Raw:', req.body);
        console.log();

        // Try to parse as JSON if Content-Type suggests it
        if (req.contentType && req.contentType.includes('application/json')) {
            try {
                const bodyData = JSON.parse(req.body);
                console.log('Parsed JSON:');
                console.log(JSON.stringify(bodyData, null, 2));
                console.log();
            } catch (e) {
                console.log('(Body is not valid JSON)');
                console.log();
            }
        }
    } else {
        console.log('=== REQUEST BODY ===');
        console.log('(No body)');
        console.log();
    }

    console.log('=================================');

    // Example: ExpressJS-like usage
    // if (req.method === 'POST' && req.path === '/deploy') {
    //     const body = JSON.parse(req.body);
    //     if (body.action === 'deploy') {
    //         console.log('Deploying...');
    //     }
    // }
    //
    // if (req.query.action === 'start') {
    //     console.log('Starting service...');
    // }

} catch (error) {
    console.error('Error parsing request data:', error.message);
    console.error('Raw input:', jsonRequestData);
    console.log('=================================');
    process.exit(1);
}
