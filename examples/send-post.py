#!/usr/bin/env python3
"""
Sends a POST request to the /python endpoint
Usage: python send-post.py <request_path> [json_payload]
"""

import sys
import json
import http.client
from datetime import datetime

def main():
    args = sys.argv[1:]
    request_path = args[0] if len(args) > 0 else None
    incoming_payload = args[1] if len(args) > 1 else None

    print('=================================')
    print('  POST Request Sender (Python)')
    print('=================================')
    print()
    print(f'Triggered from: {request_path or "(unknown)"}')
    print(f'Timestamp: {datetime.now().isoformat()}')
    print()

    # Prepare the payload to send
    payload = {
        'source': request_path,
        'timestamp': datetime.now().isoformat(),
        'message': 'This is a POST request from /python-post',
        'originalPayload': json.loads(incoming_payload) if incoming_payload else None,
        'data': {
            'user': 'automated-sender',
            'action': 'forward',
            'value': 42
        }
    }

    payload_string = json.dumps(payload)

    print('=== SENDING POST TO /python ===')
    print(f'Payload: {payload_string}')
    print()

    try:
        # Create connection
        conn = http.client.HTTPConnection('127.0.0.1', 8080)

        # Send POST request
        headers = {
            'Content-Type': 'application/json',
            'Content-Length': str(len(payload_string.encode('utf-8')))
        }

        conn.request('POST', '/python', payload_string, headers)

        # Get response
        response = conn.getresponse()

        print(f'Response Status: {response.status} {response.reason}')
        print(f'Response Headers: {dict(response.headers)}')
        print()

        response_data = response.read().decode('utf-8')

        print('=== RESPONSE ===')
        print(response_data)
        print()
        print('=================================')
        print('Request completed successfully!')

        conn.close()

    except Exception as e:
        print(f'Error sending request: {e}')

    # Keep the window open
    print()
    input('Press Enter to close this window...')

if __name__ == '__main__':
    main()
