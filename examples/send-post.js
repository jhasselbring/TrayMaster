#!/usr/bin/env node

// Sends a POST request to the /node endpoint
// Usage: node send-post.js <request_path> [json_payload]

const http = require('http');

const args = process.argv.slice(2);
const requestPath = args[0];
const incomingPayload = args[1];

console.log('=================================');
console.log('  POST Request Sender');
console.log('=================================');
console.log();
console.log('Triggered from:', requestPath || '(unknown)');
console.log('Timestamp:', new Date().toISOString());
console.log();

// Prepare the payload to send
const payload = {
    source: requestPath,
    timestamp: new Date().toISOString(),
    message: 'This is a POST request from /node-post',
    originalPayload: incomingPayload ? JSON.parse(incomingPayload) : null,
    data: {
        user: 'automated-sender',
        action: 'forward',
        value: 42
    }
};

const payloadString = JSON.stringify(payload);

console.log('=== SENDING POST TO /node ===');
console.log('Payload:', payloadString);
console.log();

const options = {
    hostname: '127.0.0.1',
    port: 8080,
    path: '/node',
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(payloadString)
    }
};

const req = http.request(options, (res) => {
    console.log(`Response Status: ${res.statusCode}`);
    console.log(`Response Headers:`, res.headers);
    console.log();

    let responseData = '';
    res.on('data', (chunk) => {
        responseData += chunk;
    });

    res.on('end', () => {
        console.log('=== RESPONSE ===');
        console.log(responseData);
        console.log();
        console.log('=================================');
        console.log('Request completed successfully!');
    });
});

req.on('error', (error) => {
    console.error('Error sending request:', error.message);
});

req.write(payloadString);
req.end();

// Keep the window open
console.log();
console.log('Press Ctrl+C to close this window...');
