#!/bin/bash
# Container run helper script for Prompt Template Manager
# Supports both Docker and Podman

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
IMAGE_NAME="proomptz:latest"
CONTAINER_NAME="proomptz"
PORT="5026"
DATA_DIR="./data"
ENV="Production"
DETACHED=true

# Function to print colored messages
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Detect container runtime (prefer Podman on Fedora/RHEL, fallback to Docker)
if command -v podman &> /dev/null; then
    CONTAINER_CMD="podman"
    print_info "Using Podman as container runtime"
elif command -v docker &> /dev/null; then
    CONTAINER_CMD="docker"
    print_info "Using Docker as container runtime"
else
    print_error "Neither Docker nor Podman is installed. Please install one first."
    exit 1
fi

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -i|--image)
            IMAGE_NAME="$2"
            shift 2
            ;;
        -n|--name)
            CONTAINER_NAME="$2"
            shift 2
            ;;
        -p|--port)
            PORT="$2"
            shift 2
            ;;
        -d|--data-dir)
            DATA_DIR="$2"
            shift 2
            ;;
        -e|--env)
            ENV="$2"
            shift 2
            ;;
        --foreground)
            DETACHED=false
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -i, --image IMAGE      Docker image (default: proomptz:latest)"
            echo "  -n, --name NAME        Container name (default: proomptz)"
            echo "  -p, --port PORT        Host port (default: 5026)"
            echo "  -d, --data-dir DIR     Data directory for SQLite (default: ./data)"
            echo "  -e, --env ENV          Environment (Development/Production, default: Production)"
            echo "  --foreground           Run in foreground (default: background)"
            echo "  -h, --help             Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                                    # Run with defaults"
            echo "  $0 -p 8080                           # Run on port 8080"
            echo "  $0 --foreground                      # Run in foreground"
            echo "  $0 -e Development                    # Run in development mode"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Create data directory if it doesn't exist
if [ ! -d "$DATA_DIR" ]; then
    print_info "Creating data directory: $DATA_DIR"
    mkdir -p "$DATA_DIR"
fi

# Check if container already exists
if $CONTAINER_CMD ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    print_warning "Container '$CONTAINER_NAME' already exists"
    read -p "Do you want to stop and remove it? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Stopping and removing existing container..."
        $CONTAINER_CMD stop $CONTAINER_NAME 2>/dev/null || true
        $CONTAINER_CMD rm $CONTAINER_NAME 2>/dev/null || true
    else
        print_error "Aborted. Please remove the container manually or use a different name."
        exit 1
    fi
fi

# Build container run command
RUN_CMD="$CONTAINER_CMD run"

if [ "$DETACHED" = true ]; then
    RUN_CMD="$RUN_CMD -d"
else
    RUN_CMD="$RUN_CMD -it --rm"
fi

RUN_CMD="$RUN_CMD --name $CONTAINER_NAME"
RUN_CMD="$RUN_CMD -p $PORT:5026"
RUN_CMD="$RUN_CMD -e ASPNETCORE_ENVIRONMENT=$ENV"
RUN_CMD="$RUN_CMD -v $(pwd)/$DATA_DIR:/app/data"
RUN_CMD="$RUN_CMD $IMAGE_NAME"

# Run the container
print_info "Starting container: $CONTAINER_NAME"
print_info "Command: $RUN_CMD"

if eval $RUN_CMD; then
    if [ "$DETACHED" = true ]; then
        print_info "Container started successfully!"
        echo ""
        print_info "Access the application:"
        echo "  - Frontend & Backend: http://localhost:$PORT"
        echo "  - Health Check: http://localhost:$PORT/health"
        echo "  - API Documentation: http://localhost:$PORT/swagger"
        echo ""
        print_info "View logs:"
        echo "  $CONTAINER_CMD logs -f $CONTAINER_NAME"
        echo ""
        print_info "Stop container:"
        echo "  $CONTAINER_CMD stop $CONTAINER_NAME"
        echo ""
        print_info "Remove container:"
        echo "  $CONTAINER_CMD rm $CONTAINER_NAME"
    fi
else
    print_error "Failed to start container!"
    exit 1
fi
