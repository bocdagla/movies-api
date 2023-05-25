using ApiApplication.ApiClients.Response;
using ProtoDefinitions;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.ApiClients.Abstractions
{
    public interface IMovieApiClient
    {
        Task<GetMovieResponse> GetAsync();
        
        Task<GetMovieResponse> GetAsync(string movieId, CancellationToken cancellationToken = default);
    }
}
