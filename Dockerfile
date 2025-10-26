# Multi-stage Dockerfile for Prompt Template Manager
# Builds both .NET backend and React frontend in a single optimized image
# Compatible with both Docker and Podman

# Stage 1: Build Frontend
FROM node:20-alpine AS frontend-build

WORKDIR /frontend

# Copy frontend package files
COPY frontend/package*.json ./

# Install dependencies
RUN npm ci --prefer-offline --no-audit

# Copy frontend source
COPY frontend/ ./

# Copy shared OpenAPI spec for API client generation
COPY shared/ /shared/

# Build frontend (includes API client generation)
RUN npm run build

# Stage 2: Build Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS backend-build

WORKDIR /backend

# Copy solution and project files
COPY backend/src/PromptTemplateManager.Api/PromptTemplateManager.Api.csproj ./src/PromptTemplateManager.Api/
COPY backend/src/PromptTemplateManager.Application/PromptTemplateManager.Application.csproj ./src/PromptTemplateManager.Application/
COPY backend/src/PromptTemplateManager.Core/PromptTemplateManager.Core.csproj ./src/PromptTemplateManager.Core/
COPY backend/src/PromptTemplateManager.Infrastructure/PromptTemplateManager.Infrastructure.csproj ./src/PromptTemplateManager.Infrastructure/

# Restore dependencies
RUN dotnet restore ./src/PromptTemplateManager.Api/PromptTemplateManager.Api.csproj

# Copy backend source
COPY backend/ ./

# Build and publish backend
RUN dotnet publish ./src/PromptTemplateManager.Api/PromptTemplateManager.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:PublishTrimmed=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# Install curl for healthcheck
RUN apk add --no-cache curl

WORKDIR /app

# Create directory for SQLite database
RUN mkdir -p /app/data

# Copy backend from build stage
COPY --from=backend-build /app/publish .

# Copy frontend build artifacts to be served by backend
COPY --from=frontend-build /frontend/dist ./wwwroot

# Set environment variables
ENV PORT=5026
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/prompttemplates.db"

# Expose port (can be overridden via --env PORT=xxxx)
EXPOSE ${PORT}

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:${PORT}/health || exit 1

# Run as non-root user for security
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "PromptTemplateManager.Api.dll"]
