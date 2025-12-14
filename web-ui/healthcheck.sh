#!/bin/sh
# Health check script for Docker container

# Check if the server is responding to HTTP requests
wget --no-verbose --tries=1 --spider http://localhost:3000/health || exit 1

exit 0
