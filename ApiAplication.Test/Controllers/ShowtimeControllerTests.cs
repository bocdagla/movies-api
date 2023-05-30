using ApiApplication.ApiClients.Abstractions;
using ApiApplication.Controllers;
using ApiApplication.Controllers.Requests;
using ApiApplication.Controllers.Requests.Showtime;
using ApiApplication.Controllers.Responses;
using ApiApplication.Database.Entities;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProtoDefinitions;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using System.Linq;
using ApiApplication.ApiClients.Response;

namespace ApiAplication.Test.Controllers
{
    public class ShowtimeControllerTests
    {
        private readonly Mock<ILogger<CreateShowtimeRequest>> _loggerMock;
        private readonly Mock<IMovieApiClient> _movieApiClientMock;
        private readonly Mock<IShowTimeService> _showTimeServiceMock;
        private readonly ShowtimeController _showtimeController;

        public ShowtimeControllerTests()
        {
            _loggerMock = new Mock<ILogger<CreateShowtimeRequest>>();
            _movieApiClientMock = new Mock<IMovieApiClient>();
            _showTimeServiceMock = new Mock<IShowTimeService>();
            _showtimeController = new ShowtimeController(_movieApiClientMock.Object, _showTimeServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Get_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var showtimeEntities = new List<ShowtimeEntity>
            {
                new ShowtimeEntity { Id = 1, AuditoriumId = 1, Movie = new MovieEntity { Title = "Movie 1" }, SessionDate = DateTime.Now }
            };
            _showTimeServiceMock.Setup(mock => mock.GetAsync(cancellationToken)).ReturnsAsync(showtimeEntities);

            // Act
            var result = await _showtimeController.Get(cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var showtimeResponses = Assert.IsAssignableFrom<IEnumerable<GetShowtimeResponse>>(okResult.Value);
            Assert.Equal(showtimeEntities.Count, showtimeResponses.Count());
        }

        [Fact]
        public async Task Get_WithInvalidData_ReturnsNotFoundResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            _showTimeServiceMock.Setup(mock => mock.GetAsync(cancellationToken)).ReturnsAsync((List<ShowtimeEntity>)null);

            // Act
            var result = await _showtimeController.Get(cancellationToken);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var showtimeEntity = new ShowtimeEntity { Id = 1, AuditoriumId = 1, Movie = new MovieEntity { Title = "Movie 1" }, SessionDate = DateTime.Now };
            _showTimeServiceMock.Setup(mock => mock.GetAsync(showtimeEntity.Id, cancellationToken)).ReturnsAsync(showtimeEntity);

            // Act
            var result = await _showtimeController.Get(showtimeEntity.Id, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var showtimeResponse = Assert.IsType<GetShowtimeResponse>(okResult.Value);
            Assert.Equal(showtimeEntity.Id, showtimeResponse.Id);
        }

        [Fact]
        public async Task Get_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            _showTimeServiceMock.Setup(mock => mock.GetAsync(It.IsAny<int>(), cancellationToken)).ReturnsAsync((ShowtimeEntity)null);

            // Act
            var result = await _showtimeController.Get(1, cancellationToken);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateShowtimeRequest { AuditoriumId = 1, MovieId = "movie1", Date = DateTime.Now };
            var movieData = new MovieData { Title = "Movie 1" };
            var createdShowtime = new ShowtimeEntity { Id = 1 };
            _movieApiClientMock.Setup(mock => mock.GetAsync(request.MovieId, cancellationToken)).ReturnsAsync(new GetMovieResponse { Success = true, Movies = new MovieData[] { movieData } });
            _showTimeServiceMock.Setup(mock => mock.CreateAsync(request.AuditoriumId, request.Date, movieData, cancellationToken)).ReturnsAsync(createdShowtime);

            // Act
            var result = await _showtimeController.Create(request, cancellationToken);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ShowtimeController.Get), createdAtActionResult.ActionName);
            var typedValue = Assert.IsType<CreatedShowtimeResponse>(createdAtActionResult.Value);
            Assert.Equal(createdShowtime.Id, typedValue.Id);
        }

        [Fact]
        public async Task Create_WithInvalidMovieId_ReturnsNotFoundResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateShowtimeRequest { AuditoriumId = 1, MovieId = "movie1", Date = DateTime.Now };
            _movieApiClientMock.Setup(mock => mock.GetAsync(request.MovieId, cancellationToken)).ReturnsAsync(new GetMovieResponse { Success = false });

            // Act
            var result = await _showtimeController.Create(request, cancellationToken);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithInvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var request = new CreateShowtimeRequest { AuditoriumId = 1, MovieId = null, Date = DateTime.Now };
            _showtimeController.ModelState.AddModelError("MovieId", "The MovieId field is required");

            // Act
            var result = await _showtimeController.Create(request, cancellationToken);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Reserve_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var showtimeId = 1;
            var resultGuid = Guid.NewGuid();
            var request = new ReserveSeatsRequest { Row = 1, SeatIds = new short[] { 1, 2, 3 } };
            _showTimeServiceMock.Setup(mock => mock.ValidateSeatsAsync(showtimeId, request.Row, request.SeatIds, cancellationToken)).ReturnsAsync(true);
            _showTimeServiceMock.Setup(mock => mock.AddReservationsAsync(showtimeId, request.Row, request.SeatIds, cancellationToken)).ReturnsAsync(resultGuid);

            // Act
            var result = await _showtimeController.Reserve(showtimeId, request, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var typedValue = Assert.IsType<AddedReservationResponse>(okResult.Value);
            Assert.Equal(resultGuid, typedValue.ReservationId);
        }

        [Fact]
        public async Task Reserve_WithInvalidSeats_ReturnsConflictResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var showtimeId = 1;
            var request = new ReserveSeatsRequest { Row = 1, SeatIds = new short[] { 1, 3 } };
            _showTimeServiceMock.Setup(mock => mock.ValidateSeatsAsync(showtimeId, request.Row, request.SeatIds, cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await _showtimeController.Reserve(showtimeId, request, cancellationToken);

            // Assert
            Assert.IsType<ConflictResult>(result);
        }

        [Fact]
        public async Task Purchase_WithValidReservationId_ReturnsOkResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PurchaseSeatsRequest { ReservationId = Guid.NewGuid() };
            var expectedResult = new PurchasedReservationResponse { ReservationId = request.ReservationId };
            _showTimeServiceMock.Setup(mock => mock.PurchaseReservation(request.ReservationId, cancellationToken)).ReturnsAsync(true);

            // Act
            var result = await _showtimeController.Purchase(request, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var typedResult = Assert.IsType<PurchasedReservationResponse>(okResult.Value);
            Assert.Equal(expectedResult.ReservationId, typedResult.ReservationId);
        }

        [Fact]
        public async Task Purchase_WithInvalidReservationId_ReturnsNotFoundResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PurchaseSeatsRequest { ReservationId = Guid.NewGuid() };
            _showTimeServiceMock.Setup(mock => mock.PurchaseReservation(request.ReservationId, cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await _showtimeController.Purchase(request, cancellationToken);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Purchase_WithInvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var request = new PurchaseSeatsRequest();
            _showtimeController.ModelState.AddModelError("ReservationId", "The ReservationId field is required");

            // Act
            var result = await _showtimeController.Purchase(request, cancellationToken);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
