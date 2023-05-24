using ApiApplication.ApiClients.Abstractions;
using ApiApplication.ApiClients.Response;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using ProtoDefinitions;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiApplication.ApiClients
{
    public class MovieApiClientGrpc : IMovieApiClient
    {
        private readonly MoviesApi.MoviesApiClient _apiClient;
        private readonly IConfiguration _conf;
        private readonly IDatabase _cache;

        public MovieApiClientGrpc(MoviesApi.MoviesApiClient moviesClient, IConfiguration conf, IDatabase cache)
        {
            _apiClient = moviesClient;
            _conf = conf;
            _cache = cache;
        }

        public async Task<GetMovieResponse> GetAllAsync()
        {
            var all = await _apiClient.GetAllAsync(new Empty(), GetDefaultHeaders());
            if (!all.Data.TryUnpack<showListResponse>(out var data))
            {
                if(_cache.SetAddAsync())
                return MapError(all);
            }
            return new GetMovieResponse {
                Success = true,
                Movies = data.Shows.Select(s => Map(s)).ToArray()
            };
        }

        public async Task<GetMovieResponse> GetMovieByIdAsync(string movieId)
        {
            var all = await _apiClient.GetByIdAsync(new IdRequest() { Id = movieId }, GetDefaultHeaders());
            if (!all.Data.TryUnpack<showResponse>(out var show))
            {
                return MapError(all);
            }
            return new GetMovieResponse
            {
                Success = true,
                Movies = new MovieData[] { Map(show) }
            };
        }

        //TODO: Move this to a mapper
        private GetMovieResponse MapError(ResponseModel all)
        {
            return new GetMovieResponse
            {
                Success = false,
                Errors = all.Exceptions
                                .Select(e => Map(e))
                                .ToArray()
            };
        }

        //TODO: Move this to a mapper
        private MovieError Map(MoviesApiException e)
        {
            return new MovieError() { Code = e.StatusCode, Message = e.Message };
        }


        //TODO: Move this to a mapper
        private MovieData Map(showResponse show)
        {
            return new MovieData()
            {
                Id = show.Id,
                FullTitle = show.FullTitle,
                Rank = Convert.ToInt32(show.Rank),
                Title = show.Title,
                Year = new DateTime(Convert.ToInt32(show.Year), 1, 1)
            };
        }

        private Metadata GetDefaultHeaders()
        {
            return new Metadata
            {
                { "X-Apikey", _conf["MovieApikey"] }
            };
        }
    }
}