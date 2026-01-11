#!/usr/bin/env node

// Test script to demonstrate HTML notifications
// This script receives the HTTP request payload and could trigger a notification
// Note: Currently, external scripts can't directly trigger notifications.
// This example shows what the payload looks like and how you could extend the system.

const args = process.argv.slice(2);
const jsonRequestData = args[0];

console.log('=== HTML Notification Test Handler ===');
console.log();

if (!jsonRequestData) {
    console.log('No request data provided');
    process.exit(1);
}

try {
    const req = JSON.parse(jsonRequestData);
    
    console.log('Received request:');
    console.log(`  Method: ${req.method}`);
    console.log(`  Path: ${req.path}`);
    console.log();
    
    // Example: Parse query parameters to get notification content
    const title = req.query?.title || 'Notification';
    const message = req.query?.message || 'This is a test notification';
    const type = req.query?.type || 'info';
    
    console.log('Would show notification:');
    console.log(`  Title: ${title}`);
    console.log(`  Message: ${message}`);
    console.log(`  Type: ${type}`);
    console.log();
    
    // In a real implementation, you would need to:
    // 1. Add a special HTTP endpoint that triggers notifications
    // 2. Or modify the Runner code to check for notification triggers in responses
    
    console.log('To actually show the notification, you would need to:');
    console.log('1. Add a /notification endpoint to the HTTP server');
    console.log('2. Or modify ExecuteMenuItem to trigger notifications');
    console.log('3. Or use the notification API directly from C# code');
    
} catch (error) {
    console.error('Error:', error.message);
    process.exit(1);
}
