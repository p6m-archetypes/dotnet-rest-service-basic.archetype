# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (for better layer caching)
COPY *.sln ./
COPY Directory.Build.props ./
COPY NuGet.config ./
COPY {{ PrefixName }}{{ SuffixName }}.API/*.csproj ./{{ PrefixName }}{{ SuffixName }}.API/
COPY {{ PrefixName }}{{ SuffixName }}.Client/*.csproj ./{{ PrefixName }}{{ SuffixName }}.Client/
COPY {{ PrefixName }}{{ SuffixName }}.Core/*.csproj ./{{ PrefixName }}{{ SuffixName }}.Core/{% if persistence != 'None' %}
COPY {{ PrefixName }}{{ SuffixName }}.Persistence/*.csproj ./{{ PrefixName }}{{ SuffixName }}.Persistence/{% endif %}
COPY {{ PrefixName }}{{ SuffixName }}.Server/*.csproj ./{{ PrefixName }}{{ SuffixName }}.Server/
COPY {{ PrefixName }}{{ SuffixName }}.IntegrationTests/*.csproj ./{{ PrefixName }}{{ SuffixName }}.IntegrationTests/
COPY {{ PrefixName }}{{ SuffixName }}.UnitTests/*.csproj ./{{ PrefixName }}{{ SuffixName }}.UnitTests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/{{ PrefixName }}{{ SuffixName }}.Server
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Create non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Install ca-certificates and curl for health checks
RUN apk add --no-cache ca-certificates curl

# Set working directory and permissions
WORKDIR /app
RUN chown -R appuser:appgroup /app

# Copy published application
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE {{ service-port }}
EXPOSE {{ management-port }}

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:{{ management-port }}/health/live || exit 1

# Environment variables for runtime configuration
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

# Set the entry point
ENTRYPOINT ["dotnet", "{{ PrefixName }}{{ SuffixName }}.Server.dll"]
