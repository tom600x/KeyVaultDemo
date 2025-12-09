# KeyVaultDemo.Api

ASP.NET Core Web API that:

- Authenticates to **Azure Key Vault** using a dedicated app registration (App A) and client secret.
- Protects endpoints with **Azure AD (App B)** using JWT bearer tokens.
- Reads an Azure SQL **connection string** from Key Vault (`SqlConnectionString`), caches it, and uses it to query a simple table.

## 1. Azure resources overview

You will configure:

- **Key Vault client app (App A)** – used by the API to access Key Vault.
- **API app (App B)** – represents the Web API. Blazor and other clients request tokens for this API.
- **Key Vault** – stores `SqlConnectionString` and is granted access to App A.
- **Azure SQL Database** – stores the `SampleItems` table.

## 2. Create Key Vault client app (App A)

1. Go to **Entra ID** > **App registrations** > **New registration**.
2. Name: `keyvault-demo-api-kv-client`.
3. Supported account types: **Single tenant** (Accounts in this organizational directory only).
4. Redirect URI: leave blank (not needed for client credentials).
5. Register.
6. Note the:
   - **Application (client) ID** – used as `KeyVaultClient:ClientId`.
   - **Directory (tenant) ID** – used as `KeyVaultClient:TenantId`.
7. Under **Certificates & secrets** > **Client secrets**, create a new secret:
   - Click **+ New client secret**
   - Description: `s1`
   - Expires: Choose appropriate expiration (e.g., 6 months)
   - Copy the **Value** (not the Secret ID) – this is `KeyVaultClient:ClientSecret`.

**Important**: App A should NOT have any API permissions configured. It uses client credentials to authenticate directly to Key Vault via RBAC.

## 3. Create API app (App B)

1. In **App registrations**, select **New registration**.
2. Name: `keyvault-demo-api`.
3. Supported account types: **Single tenant**.
4. Redirect URI: not required for this backend-only API.
5. Register.
6. Note the **Application (client) ID** – this will be used as `AzureAd:ClientId` and as the API resource identifier.

### 3.1. Expose an API (App B)
 
1. Go to **Expose an API** in the left menu
2. Click **+ Add a scope**
3. If prompted for Application ID URI:
   - Accept the default: `api://{AppB-client-id}` or customize it
   - Click **Save and continue**
4. Add a scope:
   - Scope name: `user_impersonation`
   - Who can consent: **Admins and users**
   - Admin consent display name: `Access KeyVault Demo API`
   - Admin consent description: `Allows the app to access the KeyVault Demo API on behalf of the signed-in user`
   - State: **Enabled**
   - Click **Add scope**

### 3.2. Configure API Permissions (App B)

**Important**: App B should **NOT** request permission to itself. Only keep Microsoft Graph permissions if needed.

1. Go to **API permissions**
2. If you see `keyvault-demo-api` (itself) in the list, **remove it**:
   - Click the three dots → **Remove permission**
3. Keep only: **Microsoft Graph / User.Read** (if present)

The API should **only expose scopes**, not request them.


### 3.3. Configure appsettings.json

In `src/KeyVaultDemo.Api/appsettings.json` (or better: user secrets), set:

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "<your-tenant>.onmicrosoft.com",
  "TenantId": "<tenant-guid>",
  "ClientId": "<AppB-client-id>",
  "Audience": "api://<AppB-client-id>"
}
```
 
## 4. Create Key Vault and configure access

### 4.1. Create Key Vault

1. In the Azure portal, create a **Key Vault** (if not already existing).
2. Note the **Vault URI** from the overview page (e.g., `https://your-kv-name.vault.azure.net/`).

### 4.2. Grant RBAC Permissions to App A

**Your Key Vault should be configured to use RBAC** (not legacy access policies).

1. Go to your Key Vault → **Access control (IAM)**.
2. Click **+ Add** → **Add role assignment**.
3. On the **Role** tab:
   - Select **Key Vault Secrets User** (provides read-only access to secret values).
4. On the **Members** tab:
   - Click **+ Select members**.
   - Search for `keyvault-demo-api-kv-client` (App A's display name) or paste its Client ID.
   - Select it and click **Select**.
5. Click **Review + assign** twice.
6. Wait 1-2 minutes for the permission to propagate.
 

**Important**: If you see "Access policies not available", your Key Vault is already configured for RBAC. This is correct and recommended.

### 4.3. Create the SQL Connection String Secret
  
1. In your Key Vault, go to **Secrets**
2. Click **+ Generate/Import**
3. Create a secret:
   - Name: `SqlConnectionString`
   - Value: your Azure SQL connection string, for example:

   ```text
   Server=tcp:<your-sql-server>.database.windows.net,1433;Database=KeyVaultDemoDb;User ID=<db-user>;Password=<db-password>;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
   ```

4. Click **Create**

### 4.4. Configure API to Use Key Vault

Update `KeyVaultClient` section in user secrets or appsettings.json:

 

Or in JSON format:

```json
"KeyVaultClient": {
  "TenantId": "<tenant-guid>",
  "ClientId": "<AppA-client-id>",
  "ClientSecret": "<AppA-client-secret>",
  "VaultUrl": "https://<your-kv-name>.vault.azure.net/",
  "SqlConnectionSecretName": "SqlConnectionString"
}
```

> **Note:** This demo uses client secret authentication. The API authenticates to Key Vault as App A using `ClientSecretCredential`, which means App A needs the RBAC permissions whether running locally or in Azure.

## 5. Azure SQL setup

1. Create an **Azure SQL Server** and a **database** (e.g. `KeyVaultDemoDb`).
2. Connect with your favorite tool and run:

```sql
CREATE TABLE dbo.SampleItems
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
);

INSERT INTO dbo.SampleItems (Name)
VALUES ('Item A'), ('Item B'), ('Item C');
```

3. Ensure your connection string in Key Vault points to this database and uses an account with `SELECT` permissions on `dbo.SampleItems`.

 

The API listens on `https://localhost:50162` and `http://localhost:50165`.

### Available Endpoints:

- **`GET /`** - API information and available endpoints (no auth required)
- **`GET /health`** - Health check endpoint (no auth required)
- **`GET /items`** - Retrieve sample items from Azure SQL (requires JWT bearer token from App B)

 

 