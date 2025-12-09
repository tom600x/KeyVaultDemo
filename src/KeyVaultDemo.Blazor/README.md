# KeyVaultDemo.Blazor

Blazor Server app that:

- Uses its own app registration (App C) to sign in users.
- Acquires an access token for the API (App B).
- Calls the API `/items` endpoint to retrieve sample data from Azure SQL (connection string stored in Key Vault and used by the API).

## Prerequisites

Before configuring the Blazor app, ensure you have completed the API setup (see `src/KeyVaultDemo.Api/README.md`):

- App A (Key Vault client) created
- App B (API) created and configured
- Key Vault created with RBAC permissions granted to App A
- Azure SQL database created with sample data

## 1. Create Blazor client app (App C)

1. In **Entra ID** > **App registrations** > **New registration**.
2. Name: `keyvault-demo-blazor`.
3. Supported account types: **Single tenant** (Accounts in this organizational directory only).
4. Redirect URI:
   - Type: **Web**
   - URL: `https://localhost:50163/signin-oidc`
5. Click **Register**.
6. Note:
   - **Application (client) ID** – used as `AzureAd:ClientId`.
   - **Directory (tenant) ID** – used as `AzureAd:TenantId`.
7. Under **Certificates & secrets** > **Client secrets**:
   - Click **+ New client secret**
   - Description: `cs`
   - Expires: Choose appropriate expiration
   - Click **Add**
   - Copy the **Value** – this is `AzureAd:ClientSecret`.

## 2. Grant Blazor app permission to call API (App B)

### 2.1. Add API Permission

1. In App C (Blazor) registration, go to **API permissions**.
2. Click **+ Add a permission** > **My APIs** tab.
3. Select **keyvault-demo-api** (App B).
4. Under **Delegated permissions**, check **user_impersonation**.
5. Click **Add permissions**.

### 2.2. Grant Admin Consent

**This step is critical!**

1. In the **API permissions** page, click **"Grant admin consent for [your tenant name]"**.
2. Click **Yes** to confirm.
3. Verify you see green checkmarks (✅) next to both permissions.

If you don't grant admin consent, users will get an error: `IDW10502: An MsalUiRequiredException was thrown`.
 

## 1. Create Blazor client app (App C)

1. In **Entra ID** > **App registrations** > **New registration**.
2. Name: `keyvault-demo-blazor`.
3. Supported account types: usually **Single tenant**.
4. Redirect URI:
   - Type: `Web`.
   - URL: `https://localhost:5003/signin-oidc` (adjust port as needed based on launchSettings).
5. Register.
6. Note:
   - **Application (client) ID** – used as `AzureAd:ClientId` in this project.
   - **Directory (tenant) ID** – used as `AzureAd:TenantId`.
7. Under **Certificates & secrets** > **Client secrets**, create a new secret and copy its **Value**.

## 2. Grant Blazor app permission to call API (App B)

1. In App C (Blazor) registration, go to **API permissions**.
2. Click **Add a permission** > **My APIs**.
3. Select the API app (App B, e.g. `keyvault-demo-api`).
4. Choose the scope you defined earlier (e.g. `user_impersonation`).
5. Add the permission, then **Grant admin consent** for your tenant.

## 3. Configure `appsettings.json` for Blazor

Update `src/KeyVaultDemo.Blazor/appsettings.json`:

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "<your-tenant>.onmicrosoft.com",
  "TenantId": "<tenant-guid>",
  "ClientId": "<AppC-client-id>",
  "ClientSecret": "<AppC-client-secret>",
  "CallbackPath": "/signin-oidc"
},
"Api": {
  "BaseUrl": "https://localhost:5001/",
  "Scope": "api://<AppB-client-id>/.default"
}
```

- `BaseUrl` must point to the running API.
- `Scope` should match the API resource you exposed for App B. Using `/.default` tells Entra ID to use the app’s configured delegated permissions.

## 4. Flow at runtime

1. User navigates to the Blazor app (App C).
2. User is redirected to Entra ID to sign in.
3. Once signed in, the user navigates to `/items`.
4. The page uses `ITokenAcquisition` to get an access token for `Api:Scope` and sends an HTTP GET to `Api:BaseUrl/items` with `Authorization: Bearer <token>`.
5. The API validates the token (App B), uses App A to pull the SQL connection string from Key Vault, queries Azure SQL, and returns the sample items.

 

Browse to the URL shown in the console (e.g. `https://localhost:5003`).

- Go to `/items` to trigger token acquisition and API call.

> Note: This sample uses **client secrets** for both App A and App C.  
