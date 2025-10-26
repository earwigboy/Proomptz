#!/bin/bash
# Container build helper script for Prompt Template Manager
# Supports both Docker and Podman

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
IMAGE_NAME="proomptz"
TAG="latest"
PLATFORM=""
BUILD_ARGS=""

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
    echo ""
    echo "Install Podman (recommended for Fedora):"
    echo "  - Fedora/RHEL: sudo dnf install podman"
    echo ""
    echo "Or install Docker:"
    echo "  - Ubuntu/Debian: https://docs.docker.com/engine/install/ubuntu/"
    echo "  - Fedora: sudo dnf install docker docker-compose"
    echo "  - macOS: https://docs.docker.com/desktop/install/mac-install/"
    echo "  - Windows: https://docs.docker.com/desktop/install/windows-install/"
    exit 1
fi

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -t|--tag)
            TAG="$2"
            shift 2
            ;;
        -n|--name)
            IMAGE_NAME="$2"
            shift 2
            ;;
        --platform)
            PLATFORM="--platform $2"
            shift 2
            ;;
        --no-cache)
            BUILD_ARGS="$BUILD_ARGS --no-cache"
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -t, --tag TAG          Image tag (default: latest)"
            echo "  -n, --name NAME        Image name (default: proomptz)"
            echo "  --platform PLATFORM    Target platform (e.g., linux/amd64,linux/arm64)"
            echo "  --no-cache             Build without using cache"
            echo "  -h, --help             Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                                    # Build proomptz:latest"
            echo "  $0 -t v1.0.0                         # Build proomptz:v1.0.0"
            echo "  $0 --platform linux/amd64            # Build for specific platform"
            echo "  $0 --no-cache                        # Build without cache"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Build the image
print_info "Building container image: $IMAGE_NAME:$TAG"
print_info "Build arguments: $BUILD_ARGS $PLATFORM"

if $CONTAINER_CMD build $BUILD_ARGS $PLATFORM -t $IMAGE_NAME:$TAG .; then
    print_info "Build successful!"
    echo ""
    print_info "Image details:"
    $CONTAINER_CMD images $IMAGE_NAME:$TAG
    echo ""
    print_info "To run the container:"
    echo "  $CONTAINER_CMD run -d -p 5026:5026 -e PORT=5026 -v \$(pwd)/data:/app/data $IMAGE_NAME:$TAG"
    echo ""
    print_info "To run on a different port (e.g., 8080):"
    echo "  $CONTAINER_CMD run -d -p 8080:8080 -e PORT=8080 -v \$(pwd)/data:/app/data $IMAGE_NAME:$TAG"
    echo ""
    print_info "Or use compose:"
    if [ "$CONTAINER_CMD" = "podman" ]; then
        echo "  podman compose up -d"
    else
        echo "  docker-compose up -d"
    fi
else
    print_error "Build failed!"
    exit 1
fi
