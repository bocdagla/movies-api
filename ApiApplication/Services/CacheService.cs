using ApiApplication.Services.Abstractions;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiApplication.Services
{
    public class CacheService : ICacheService
    {
        private IDatabase _cache;

        public CacheService(IDatabase database)
        {
            _cache = database;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _cache.KeyExistsAsync(key);
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            var json = await _cache.StringGetAsync(key);
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task SetAsync<T>(string key, T val) where T : class
        {
            await _cache.SetAddAsync(key, JsonSerializer.Serialize(val));
        }
    }
}
