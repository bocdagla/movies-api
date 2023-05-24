using ApiApplication.ApiClients.Response;
using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.ApiClients.Abstractions
{
    public interface IMovieApiClient
    {
        Task<GetMovieResponse> GetAllAsync();
        
        Task<GetMovieResponse> GetMovieByIdAsync(string movieId);
    }
}
