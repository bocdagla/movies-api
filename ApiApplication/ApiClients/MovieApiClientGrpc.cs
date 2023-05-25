using ApiApplication.ApiClients.Abstractions;
using ApiApplication.ApiClients.Response;
using ApiApplication.Services.Abstractions;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using ProtoDefinitions;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.ApiClients
{
    public class MovieApiClientGrpc : IMovieApiClient
    {
        private readonly MoviesApi.MoviesApiClient _apiClient;
        private readonly IConfiguration _conf;
        private readonly ICacheService _cache;

        public MovieApiClientGrpc(MoviesApi.MoviesApiClient moviesClient, IConfiguration conf, ICacheService cache)
        {
            _apiClient = moviesClient;
            _conf = conf;
            _cache = cache;
        }

        public async Task<GetMovieResponse> GetAsync()
        {
            var all = await _apiClient.GetAllAsync(new Empty(), GetDefaultHeaders());
            if (!all.Data.TryUnpack<showListResponse>(out var data))
            {
                return MapError(all);
            }
            return MapSuccess(data);
        }

        public async Task<GetMovieResponse> GetAsync(string movieId, CancellationToken cancellationToken = default)
        {
            var existsInCache= _cache.ExistsAsync(movieId);
            var all = await _apiClient.GetByIdAsync(new IdRequest() { Id = movieId }, GetDefaultHeaders(), cancellationToken: cancellationToken);
            if (!all.Data.TryUnpack<showResponse>(out var show) || !all.Success)
            {
                if (!await existsInCache)
                {//Just returning the errors so we can deal with the problems in the caller
                    return MapError(all);
                }
                show = await _cache.GetAsync<showResponse>(movieId);
            }
            else
            {//We could just not await this operation, though it may lead to uncontrolled errors at cost of very little performance 
                await _cache.SetAsync(movieId, JsonSerializer.Serialize(show));
            }

            return MapSuccess(show);
        }

        private GetMovieResponse MapSuccess(showListResponse shows)
        {
            return new GetMovieResponse
            {
                Success = true,
                Movies = shows.Shows.Select(Map).ToArray()
            };
        }

        private GetMovieResponse MapSuccess(showResponse show)
        {
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
                                .Select(Map)
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
                ImdbRating = show.ImDbRating,
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