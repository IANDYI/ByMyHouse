#!/bin/bash

# BuyMyHouse - Start All Services Script (Mac/Linux)
# This script starts all required services for local development

echo "========================================"
echo "  BuyMyHouse - Starting All Services"
echo "========================================"
echo ""

# Get the script directory (project root)
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Project Root: $PROJECT_ROOT"
echo ""
echo "Starting services in the background..."
echo "Logs will be displayed in this terminal."
echo ""

# Check if Azurite is installed
if ! command -v azurite &> /dev/null; then
    echo "⚠️  WARNING: Azurite not found!"
    echo "Install it with: npm install -g azurite"
    echo "Or install the Azurite VS Code extension"
    echo ""
fi

# Start Azurite
echo "[1/4] Starting Azurite (Azure Storage Emulator)..."
if command -v azurite &> /dev/null; then
    AZURITE_DIR="$HOME/azurite"
    mkdir -p "$AZURITE_DIR"
    
    # Start in background and capture PID
    (cd "$PROJECT_ROOT" && azurite --silent --location "$AZURITE_DIR" > /tmp/azurite.log 2>&1 &)
    echo "  ✓ Azurite started (PID: $!)"
    sleep 3
else
    echo "  ✗ Skipping Azurite (not installed)"
fi

# Start Listings API
echo "[2/4] Starting Listings API (Port 5001)..."
LISTINGS_PATH="$PROJECT_ROOT/BuyMyHouse.Listings"
(cd "$LISTINGS_PATH" && dotnet run > /tmp/listings.log 2>&1 &)
echo "  ✓ Listings API starting (PID: $!)"
echo "  → Logs: tail -f /tmp/listings.log"
sleep 5

# Start Mortgage API
echo "[3/4] Starting Mortgage API (Port 5002)..."
MORTGAGE_PATH="$PROJECT_ROOT/BuyMyHouse.Mortgage"
(cd "$MORTGAGE_PATH" && dotnet run > /tmp/mortgage.log 2>&1 &)
echo "  ✓ Mortgage API starting (PID: $!)"
echo "  → Logs: tail -f /tmp/mortgage.log"
sleep 5

# Start Azure Functions
echo "[4/4] Starting Azure Functions (Port 7071)..."
if command -v func &> /dev/null; then
    FUNCTIONS_PATH="$PROJECT_ROOT/BuyMyHouse.Functions"
    (cd "$FUNCTIONS_PATH" && func start > /tmp/functions.log 2>&1 &)
    echo "  ✓ Azure Functions starting (PID: $!)"
    echo "  → Logs: tail -f /tmp/functions.log"
else
    echo "  ✗ WARNING: Azure Functions Core Tools not found!"
    echo "  Install with: npm install -g azure-functions-core-tools@4 --unsafe-perm true"
fi

sleep 3

echo ""
echo "========================================"
echo "  All Services Started!"
echo "========================================"
echo ""
echo "🌐 Access the services at:"
echo "  • Listings API:  https://localhost:5001/swagger"
echo "  • Mortgage API:  https://localhost:5002/swagger"
echo "  • Azure Functions: http://localhost:7071"
echo ""
echo "📋 View logs:"
echo "  • Listings API:  tail -f /tmp/listings.log"
echo "  • Mortgage API:  tail -f /tmp/mortgage.log"
echo "  • Azure Functions: tail -f /tmp/functions.log"
echo ""

# Open browsers if on Mac and Terminal.app has permission
if [[ "$OSTYPE" == "darwin"* ]]; then
    sleep 2
    open "https://localhost:5001/swagger" 2>/dev/null || echo "  → Open https://localhost:5001/swagger manually"
    sleep 1
    open "https://localhost:5002/swagger" 2>/dev/null || echo "  → Open https://localhost:5002/swagger manually"
fi

echo ""
echo "💡 Tip: Check README.md for testing instructions"
echo ""
echo "To stop all services, run: ./stop-all.sh"
echo ""
echo "Waiting for services to fully start..."
sleep 5
echo "✅ Services should be ready!"
echo ""