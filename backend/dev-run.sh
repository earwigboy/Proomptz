#!/bin/bash
# Helper script to run the backend API in Development mode
# This ensures appsettings.Development.json is loaded

set -e

# Set environment to Development
export ASPNETCORE_ENVIRONMENT=Development

# Optional: Set custom port if provided as argument
if [ -n "$1" ]; then
    export PORT=$1
    echo "Running backend on port $PORT in Development mode..."
else
    echo "Running backend on default port (5026) in Development mode..."
fi

# Run the API
dotnet run --project src/PromptTemplateManager.Api
