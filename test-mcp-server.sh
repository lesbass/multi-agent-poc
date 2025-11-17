#!/bin/bash

# Test script for MCP Server
echo "Testing MCP Server - Pluto Tool"
echo "================================"
echo ""

# Build the project
echo "Building McpServer..."
dotnet build McpServer/McpServer.csproj --verbosity quiet
if [ $? -ne 0 ]; then
    echo "Build failed"
    exit 1
fi
echo "Build successful!"
echo ""

# Start the server in background and test it
echo "Starting MCP Server..."
cd McpServer

# Test 1: Initialize
echo "Test 1: Initialize"
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run --no-build 2>/dev/null &
SERVER_PID=$!
sleep 1

# Kill the server
kill $SERVER_PID 2>/dev/null

echo ""
echo "To manually test the server, run:"
echo "  cd McpServer && dotnet run"
echo ""
echo "Then send JSON-RPC requests via stdin, for example:"
echo '  {"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}'
echo '  {"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
echo '  {"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"pluto","arguments":{"a":5,"b":3}}}'
