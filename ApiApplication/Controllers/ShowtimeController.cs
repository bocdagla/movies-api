using ApiApplication.ApiClients.Abstractions;
using ApiApplication.Controllers.Requests.Showtime;
using ApiApplication.Controllers.Responses;
using ApiApplication.Database.Entities;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Controllers
{
    [Route("showtime")]
    public class ShowtimeController : Controller
    {
        private ILogger _log;
        private IMovieApiClient _movieApiClient;
        private IShowTimeService _showTimeService;

        public ShowtimeController(IMovieApiClient moviesClient, IShowTimeService showTimeService, ILogger<CreateShowtimeRequest> log)
        {
            _movieApiClient = moviesClient;
            _showTimeService = showTimeService;
            _log = log;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetShowtimeResponse>))]
        [Route("")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _showTimeService.GetAsync(cancellationToken);
            if(result == null)
            {
                return NotFound();
            }
            return Ok(result.Select(Map));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetShowtimeResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _showTimeService.GetAsync(id, cancellationToken);
            return Ok(Map(result));
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateShowtimeRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest();
            var movie = await _movieApiClient.GetAsync(request.MovieId, cancellationToken);
            if(!movie.Success)
            {
                return NotFound();
            }
            var result = await _showTimeService.CreateAsync(request.AuditoriumId, request.Date, movie.Movies[0], cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Id });
        }

        [HttpPatch]
        [Route("{id}/reserve")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reserve(int id, [FromBody] ReserveSeatsRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest();
            if(!await _showTimeService.ValidateSeatsAsync(id, request.Row, request.SeatIds, cancellationToken))
            {
                return Conflict();
            }
            
            var reservationId = await _showTimeService.AddReservationsAsync(id, request.Row, request.SeatIds, cancellationToken);
            return Ok(reservationId);
        }

        [HttpPatch]
        [Route("/purchase")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Purchase([FromBody] PurchaseSeatsRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!await _showTimeService.PurchaseReservation(request.ReservationId, cancellationToken))
            {
                return NotFound();
            }
            return Ok(request.ReservationId);
        }

        //Todo: Move this to a mapper
        private GetShowtimeResponse Map(ShowtimeEntity entity)
        {
            return new GetShowtimeResponse()
            {
                Id = entity.Id,
                AuditoriumId = entity.AuditoriumId,
                MovieTitle = entity.Movie.Title,
                SessionDate = entity.SessionDate
            };
        }
    }
}
