# Runtime-only stage - expects pre-built artifacts from CI/CD
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Create non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Install ca-certificates and curl for health checks
RUN apk add --no-cache ca-certificates curl

# Set working directory and permissions
WORKDIR /app
RUN chown -R appuser:appgroup /app

# Copy pre-built application (from CI/CD artifacts)
COPY --chown=appuser:appgroup ./publish .

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 5030
EXPOSE 5031

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5031/health/live || exit 1

# Environment variables for runtime configuration
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5030;http://+:5031
ENV DOTNET_EnableDiagnostics=0

# Set the entry point
ENTRYPOINT ["dotnet", "{{ PrefixName }}{{ SuffixName }}.Server.dll"]