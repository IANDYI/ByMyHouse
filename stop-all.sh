#!/bin/bash

# BuyMyHouse - Stop All Services Script
# This script stops all running services

echo "========================================"
echo "  BuyMyHouse - Stopping All Services"
echo "========================================"
echo ""

# Kill all processes by pattern
echo "Stopping all services..."

# Kill Azurite
pkill -9 -f azurite 2>/dev/null && echo "  ✓ Azurite stopped" || echo "  ✗ Azurite not running"

# Kill dotnet processes and BuyMyHouse executables
pkill -9 -f "BuyMyHouse.Listings" 2>/dev/null && echo "  ✓ Listings API stopped" || echo "  ⚠️  Listings API not running via executable"
pkill -9 -f "BuyMyHouse.Mortgage" 2>/dev/null && echo "  ✓ Mortgage API stopped" || echo "  ⚠️  Mortgage API not running via executable"
pkill -9 -f "dotnet run" 2>/dev/null && echo "  ✓ .NET APIs stopped" || echo "  ✗ .NET APIs not running via dotnet"

# Kill Azure Functions
pkill -9 -f "func start" 2>/dev/null && echo "  ✓ Azure Functions stopped" || echo "  ✗ Azure Functions not running"

# Clean up any remaining dotnet processes
pkill -9 -f "MSBuild.dll" 2>/dev/null
pkill -9 -f "VBCSCompiler" 2>/dev/null

sleep 1

echo ""
echo "========================================"
echo "  All Services Stopped!"
echo "========================================"
echo ""

# Verify no processes are running
if ps aux | grep -E "(azurite|BuyMyHouse|dotnet|func)" | grep -v grep | grep -v "stop-all"; then
    echo "⚠️  Warning: Some processes may still be running"
    echo "Remaining processes:"
    ps aux | grep -E "(azurite|BuyMyHouse|dotnet|func)" | grep -v grep | grep -v "stop-all"
    echo ""
    echo "Run: pkill -9 -f BuyMyHouse"
else
    echo "✅ All processes have been stopped successfully"
fi

echo ""