# KeyVaultDemo.Api.Tests

NUnit-based test project for `KeyVaultDemo.Api`.

- Uses `WebApplicationFactory<Program>` to host the API in-memory.
- Replaces `SecretClient` with a mock so tests do not require real Azure Key Vault or SQL.

## Running tests

From the solution root:

```powershell
cd C:\Users\thordill\source\repos\KeyVaultAPI

dotnet test KeyVaultDemo.sln
```

You can extend these tests to:

- Validate behavior when Key Vault is unavailable.
- Validate caching logic in `SqlConnectionCache`.
- Add integration-style tests that use a real Key Vault and Azure SQL (if desired), controlled by configuration.
