using Android.Content;
using MoneyTracker.Presentation.Services.Interfaces;

namespace MoneyTracker.Presentation.Services;

public class AndroidCacheService : ICacheService
{
    private readonly ISharedPreferences _preferences;
    private readonly string _cachePrefix = "cache_";

    public AndroidCacheService(Context context)
    {
        _preferences = context.GetSharedPreferences("app_cache", FileCreationMode.Private);
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var json = _preferences.GetString(_cachePrefix + key, null);
            if (string.IsNullOrEmpty(json))
                return Task.FromResult<T?>(null);

            // Check expiry
            var expiryKey = _cachePrefix + key + "_expiry";
            var expiryTicks = _preferences.GetLong(expiryKey, 0);

            if (expiryTicks > 0 && DateTime.UtcNow.Ticks > expiryTicks)
            {
                // Expired, remove it
                RemoveAsync(key);
                return Task.FromResult<T?>(null);
            }

            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache get error: {ex.Message}");
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            var editor = _preferences.Edit();

            editor?.PutString(_cachePrefix + key, json);

            if (expiry.HasValue)
            {
                var expiryTicks = DateTime.UtcNow.Add(expiry.Value).Ticks;
                editor?.PutLong(_cachePrefix + key + "_expiry", expiryTicks);
            }

            editor?.Apply();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache set error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        var editor = _preferences.Edit();
        editor?.Remove(_cachePrefix + key);
        editor?.Remove(_cachePrefix + key + "_expiry");
        editor?.Apply();

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        var editor = _preferences.Edit();

        // Remove all cache keys
        var allKeys = _preferences.All?.Keys;
        if (allKeys != null)
        {
            foreach (var key in allKeys.Where(k => k.StartsWith(_cachePrefix)))
            {
                editor?.Remove(key);
            }
        }

        editor?.Apply();
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        var exists = _preferences.Contains(_cachePrefix + key);
        return Task.FromResult(exists);
    }
}