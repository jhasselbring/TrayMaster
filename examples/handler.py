#!/usr/bin/env python3
"""
Python HTTP Request Handler
Usage: python handler.py <json_request_data>

The request data is a JSON object containing:
- method: HTTP method (GET, POST, etc.)
- path: URL path
- query: Query parameters dict
- headers: Request headers dict
- body: Request body string
- contentType: Content-Type header value
- remoteIp: Client IP address
- url: Full request URL
- receivedAt: Timestamp
"""

import sys
import json
from datetime import datetime

def main():
    args = sys.argv[1:]
    json_request_data = args[0] if len(args) > 0 else None

    print('=================================')
    print('  Python HTTP Request Handler')
    print('=================================')
    print()

    if not json_request_data:
        print('No request data provided')
        print('=================================')
        sys.exit(1)

    try:
        # Parse the request data (ExpressJS-like format)
        req = json.loads(json_request_data)

        print('=== REQUEST INFORMATION ===')
        print(f"Method: {req.get('method', 'N/A')}")
        print(f"Path: {req.get('path', 'N/A')}")
        print(f"URL: {req.get('url', 'N/A')}")
        print(f"Remote IP: {req.get('remoteIp', 'N/A')}")
        print(f"Content-Type: {req.get('contentType', 'N/A')}")
        print(f"Received At: {req.get('receivedAt', 'N/A')}")
        print()

        # Query parameters (ExpressJS req.query)
        query = req.get('query', {})
        if query:
            print('=== QUERY PARAMETERS ===')
            for key, value in query.items():
                print(f'  {key}: {value}')
            print()

        # Headers (ExpressJS req.headers)
        headers = req.get('headers', {})
        if headers:
            print('=== REQUEST HEADERS ===')
            for key, value in headers.items():
                print(f'  {key}: {value}')
            print()

        # Body (ExpressJS req.body)
        body = req.get('body', '')
        if body:
            print('=== REQUEST BODY ===')
            print(f'Raw: {body}')
            print()

            # Try to parse as JSON if Content-Type suggests it
            content_type = req.get('contentType', '')
            if 'application/json' in content_type:
                try:
                    body_data = json.loads(body)
                    print('Parsed JSON:')
                    print(json.dumps(body_data, indent=2))
                    print()
                except json.JSONDecodeError:
                    print('(Body is not valid JSON)')
                    print()
        else:
            print('=== REQUEST BODY ===')
            print('(No body)')
            print()

        print('=================================')

        # Example: ExpressJS-like usage
        # if req.get('method') == 'POST' and req.get('path') == '/deploy':
        #     body_data = json.loads(req.get('body', '{}'))
        #     if body_data.get('action') == 'deploy':
        #         print('Deploying...')
        #
        # if req.get('query', {}).get('action') == 'start':
        #     print('Starting service...')

    except json.JSONDecodeError as e:
        print(f'Error parsing request data: {e}')
        print(f'Raw input: {json_request_data}')
        print('=================================')
        sys.exit(1)

if __name__ == '__main__':
    main()
