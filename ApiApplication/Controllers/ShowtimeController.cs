using ApiApplication.ApiClients.Abstractions;
using ApiApplication.Controllers.Requests;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Services.Abstractions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProtoDefinitions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiApplication.Controllers
{
    [Route("showtime")]
    public class ShowtimeController : Controller
    {
        private ILogger _log;
        private IMovieApiClient _movieApiClient;
        private IShowTimeService _showTimeService;

        public ShowtimeController(IMovieApiClient moviesClient, IShowTimeService showTimeService, ILogger log)
        {
            _movieApiClient = moviesClient;
            _showTimeService = showTimeService;
            _log = log;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _movieApiClient.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] CreateShowtimeRequest request)
        {
            var movie = await _movieApiClient.GetMovieByIdAsync(request.MovieId);
            if(!movie.Success)
            {

            }
            _showTimeService.Create(request.AuditoriumId, request.Date, movie);
            return Ok();
        }
    }
}
