#!/usr/bin/env node

// Simple request viewer - just prints the whole request object
const args = process.argv.slice(2);
const jsonRequestData = args[0];

if (!jsonRequestData) {
    console.log('No request data provided');
    process.exit(1);
}

// Check if we received the old format (just a path string) vs new format (JSON)
if (jsonRequestData.startsWith('/') && !jsonRequestData.startsWith('{')) {
    console.error('ERROR: Received old format (path string) instead of JSON.');
    console.error('This usually means the Runner app needs to be rebuilt.');
    console.error('Received:', jsonRequestData);
    process.exit(1);
}

try {
    const req = JSON.parse(jsonRequestData);
    console.log(JSON.stringify(req, null, 2));
} catch (error) {
    console.error('Error parsing request data:', error.message);
    console.error('Raw input:', jsonRequestData);
    process.exit(1);
}
