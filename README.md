
# SW Surl - URL Shortening Service

[![Build Status](https://github.com/simplify9/SW-Surl-api/workflows/CI%2FCD%20Pipeline/badge.svg)](https://github.com/simplify9/SW-Surl-api/actions)
[![NuGet Version](https://img.shields.io/nuget/v/SimplyWorks.Surl.Sdk.svg)](https://www.nuget.org/packages/SimplyWorks.Surl.Sdk)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SimplyWorks.Surl.Sdk.svg)](https://www.nuget.org/packages/SimplyWorks.Surl.Sdk)
[![Docker Image](https://img.shields.io/badge/docker-ghcr.io%2Fsimplify9%2Fsw--surl--api-blue)](https://ghcr.io/simplify9/sw-surl-api)
[![Helm Chart](https://img.shields.io/badge/helm-ghcr.io%2Fsimplify9%2Fcharts%2Fsurl-blue)](https://ghcr.io/simplify9/charts/surl)
[![License](https://img.shields.io/github/license/simplify9/SW-Surl-api)](https://github.com/simplify9/SW-Surl-api/blob/main/LICENSE)

| **Package** |**Version** |**Downloads** |
| ------- | ----- | ----- |
| `SimplyWorks.Surl.Sdk` | [![NuGet](https://img.shields.io/nuget/v/SimplyWorks.Surl.Sdk.svg)](https://nuget.org/packages/SimplyWorks.Surl.Sdk) | [![Nuget](https://img.shields.io/nuget/dt/SimplyWorks.Surl.Sdk.svg)](https://nuget.org/packages/SimplyWorks.Surl.Sdk) |

SW Surl is a lightweight, high-performance URL shortening service built with .NET 8. It provides a simple API for creating and managing short URLs with automatic redirection capabilities.

## Features

- 🚀 **Fast & Lightweight**: Built with .NET 8 for optimal performance
- 🔗 **URL Shortening**: Create short, manageable URLs from long ones
- 🔄 **Automatic Redirection**: Seamless redirection from short to original URLs
- 📦 **SDK Available**: Easy integration with .NET applications
- 🐳 **Docker Ready**: Containerized and ready for deployment
- ☸️ **Kubernetes Native**: Helm chart included for easy deployment
- 🗄️ **Database Agnostic**: Configurable database connection

## CI/CD Configuration

This project uses the Simplify9 reusable workflow `sw-cicd.yml` for automated deployment. The workflow is configured to:

- Build and test the .NET application
- Package the NuGet SDK
- Build and push Docker images to GitHub Container Registry
- Deploy to Kubernetes using Helm charts
- Handle database connection strings securely through GitHub secrets

### Required GitHub Secrets

The following secrets must be configured in your repository or organization:

#### Organization Secrets
- `S9Dev_KUBECONFIG`: Base64 encoded kubeconfig for Kubernetes deployment
- `SWNUGETKEY`: NuGet API key for package publishing
- `S9_GITHUB_TOKEN`: GitHub token for workflow operations

#### Repository Secrets
- `DBCS_ESCAPED`: Database connection string (properly escaped for Helm)

### Deployment Configuration

The application is deployed with the following configuration:
- **Environment**: Staging
- **Namespace**: playground
- **Ingress**: Enabled with TLS (surl.sf9.io)
- **Database**: Connection string passed securely via Helm values
- **Registry**: GitHub Container Registry (ghcr.io)

## Quick Start

### Using Docker

```bash
# Pull the latest image
docker pull ghcr.io/simplify9/sw-surl-api:latest

# Run with basic configuration
docker run -p 8080:80 \
  -e ConnectionStrings__SurlDb="your-connection-string" \
  -e Token__Key="your-secret-key" \
  -e Token__Issuer="your-issuer" \
  -e Token__Audience="your-audience" \
  ghcr.io/simplify9/sw-surl-api:latest
```

### Using Helm Chart

```bash
# Add the chart repository (if using OCI registry)
helm install surl oci://ghcr.io/simplify9/charts/surl \
  --set db="your-connection-string" \
  --set global.token.key="your-secret-key" \
  --set global.token.issuer="your-issuer" \
  --set global.token.audience="your-audience"
```

## API Reference

### Endpoints

#### Create Short URL
```http
POST /ShortUrls
Content-Type: application/json

{
  "fullUrl": "https://example.com/very/long/url/that/needs/shortening"
}
```

**Response:**
```json
{
  "id": "abc123",
  "fullUrl": "https://example.com/very/long/url/that/needs/shortening",
  "shortUrl": "https://your-domain.com/abc123"
}
```

#### Retrieve URL
```http
GET /ShortUrls/{key}
```

**Response:**
```json
{
  "id": "abc123", 
  "fullUrl": "https://example.com/very/long/url/that/needs/shortening",
  "shortUrl": "https://your-domain.com/abc123"
}
```

#### Redirect (Middleware)
```http
GET /{key}
```
Automatically redirects to the original URL.

## Helm Chart Configuration

### Installation

```bash
# Install with default values
helm install my-surl oci://ghcr.io/simplify9/charts/surl

# Install with custom values
helm install my-surl oci://ghcr.io/simplify9/charts/surl -f values.yaml
```

### Configuration Values

| Parameter | Description | Default |
|-----------|-------------|---------|
| `replicaCount` | Number of replicas | `1` |
| `image.repository` | Docker image repository | `simplify9/sw-surl-api` |
| `image.tag` | Image tag | `""` (uses appVersion) |
| `image.pullPolicy` | Image pull policy | `IfNotPresent` |
| `service.type` | Kubernetes service type | `ClusterIP` |
| `service.port` | Service port | `80` |
| `ingress.enabled` | Enable ingress | `false` |
| `ingress.annotations` | Ingress annotations | `{}` |
| `ingress.hosts` | Ingress hosts | `[]` |
| `ingress.tls` | Ingress TLS configuration | `[]` |
| `resources` | Resource limits and requests | `{}` |
| `nodeSelector` | Node selector | `{}` |
| `tolerations` | Pod tolerations | `[]` |
| `affinity` | Pod affinity | `{}` |

### Required Configuration

| Parameter | Description | Required |
|-----------|-------------|----------|
| `db` | Database connection string | ✅ |
| `global.token.key` | JWT token signing key | ✅ |
| `global.token.issuer` | JWT token issuer | ✅ |
| `global.token.audience` | JWT token audience | ✅ |

### Example values.yaml

```yaml
# Basic configuration
replicaCount: 2

# Database connection
db: "Server=localhost;Database=SurlDb;User=sa;Password=YourPassword;"

# JWT configuration
global:
  environment: Production
  token:
    key: "your-super-secret-jwt-signing-key-here"
    issuer: "https://your-domain.com"
    audience: "surl-api"

# Service configuration
service:
  type: LoadBalancer
  port: 80

# Ingress configuration
ingress:
  enabled: true
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    cert-manager.io/cluster-issuer: letsencrypt
  hosts:
    - host: surl.your-domain.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: surl-tls
      hosts:
        - surl.your-domain.com

# Resource limits
resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 100m
    memory: 128Mi

# Health checks
probes:
  enabled: true
```

## Development

### Prerequisites

- .NET 8 SDK
- SQL Server (or compatible database)

### Running Locally

```bash
# Clone the repository
git clone https://github.com/simplify9/SW-Surl-api.git
cd SW-Surl-api

# Restore dependencies
dotnet restore

# Update connection string in appsettings.json
# Run the application
dotnet run --project SW.Surl.Web
```

### Using the SDK

```csharp
using SW.Surl.Sdk;

// Initialize the client
var client = new SurlClient(new SurlClientOptions
{
    BaseUrl = "https://your-surl-api.com",
    ApiKey = "your-api-key"
});

// Create a short URL
var shortUrl = await client.CreateShortUrlAsync(new ShortUrlRequest
{
    FullUrl = "https://example.com/very/long/url"
});

// Retrieve URL information
var urlInfo = await client.GetShortUrlAsync(shortUrl.Id);
```


