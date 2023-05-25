using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Services.Abstractions
{
    public interface ICacheService
    {
        Task<bool> ExistsAsync(string key);
        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T val) where T : class;
    }
}
