# Configuration Guide

## Application Settings

The application uses ASP.NET Core's configuration system with different settings files for different environments.

### Files

- **appsettings.json** - Base configuration with placeholder values (committed to git)
- **appsettings.Development.json** - Development environment secrets (NOT committed to git)
- **appsettings.Production.json** - Production environment secrets (NOT committed to git)

### Setup for Development

1. Copy `appsettings.json` to `appsettings.Development.json`
2. Replace all placeholder values with your actual credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;..."
  },
  "Jwt": {
    "SecretKey": "your-actual-jwt-secret"
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "AzureStorage": {
    "ConnectionString": "your-azure-connection-string",
    ...
  },
  "Email": {
    "ResendApiKey": "your-resend-api-key"
  }
}
```

### Security Notes

- **NEVER** commit `appsettings.Development.json` or `appsettings.Production.json` to git
- These files are already in `.gitignore`
- All sensitive credentials should only be in environment-specific files
- For production, use environment variables or Azure Key Vault instead of config files

### Required Secrets

The following secrets need to be configured:

1. **Database Connection** - SQL Server connection string
2. **JWT Secret** - Secret key for JWT token generation
3. **Google OAuth** - Client ID and Client Secret from Google Cloud Console
4. **Azure Storage** - Connection string and account details for blob storage
5. **Email Provider** - Resend API key for sending emails
6. **Odoo Integration** - Odoo instance URL, database, username, and API key

### Environment Variables (Alternative)

Instead of using appsettings files, you can set environment variables:

```bash
ConnectionStrings__DefaultConnection="Server=..."
Jwt__SecretKey="your-secret"
GoogleAuth__ClientId="your-id"
GoogleAuth__ClientSecret="your-secret"
```

Environment variables override appsettings values.
