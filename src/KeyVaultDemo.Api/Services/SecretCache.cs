namespace KeyVaultDemo.Api.Services;

/// <summary>
/// Simple in-memory cache for Key Vault secrets, keyed by secret name.
/// </summary>
public class SecretCache
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly object _lock = new();

    public async Task<string> GetOrAddAsync(string secretName, Func<Task<string>> factory)
    {
        if (_cache.TryGetValue(secretName, out var value))
        {
            return value;
        }

        value = await factory();

        lock (_lock)
        {
            if (!_cache.ContainsKey(secretName))
            {
                _cache[secretName] = value;
            }

            return _cache[secretName];
        }
    }
}
