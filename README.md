# KeyVaultDemo Solution

This solution demonstrates how to:

- Authenticate an ASP.NET Core Web API to Azure Key Vault using a **separate app registration (App A)** and a **client secret**.
- Protect the API (App B) with Azure AD (Entra ID) and require **bearer tokens**.
- Use a **Blazor Server** app (App C) as a client that:
  - Signs in a user.
  - Acquires an access token for the API.
  - Calls the API to retrieve sample data from Azure SQL, where the **connection string is stored in Key Vault**.
- Test the API with a separate test project.

Projects:

- `src/KeyVaultDemo.Api` – Web API using Key Vault + Azure SQL.
- `src/KeyVaultDemo.Blazor` – Blazor Server app that signs in the user and calls the API.
- `tests/KeyVaultDemo.Api.Tests` – NUnit tests for the API.

See:

- `src/KeyVaultDemo.Api/README.md` for API, Key Vault, and Azure SQL setup.
- `src/KeyVaultDemo.Blazor/README.md` for Blazor client and auth setup.
- `tests/KeyVaultDemo.Api.Tests/README.md` for test project notes.
