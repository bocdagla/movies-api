using ApiApplication.ApiClients.Abstractions;
using ApiApplication.Controllers;
using ApiApplication.Controllers.Requests;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProtoDefinitions;
using System.Net;
using Xunit;

namespace ApiAplication.Test.Controllers
{
    public class ShowtimeControllerTests
    {
        Mock<ILogger> _logMock;
        Mock<IMovieApiClient> _apiMock;
        Mock<IShowTimeService> _serviceMock;

        public ShowtimeControllerTests()
        {
            _logMock = new Mock<ILogger>();
            _apiMock = new Mock<IMovieApiClient>();
            _serviceMock = new Mock<IShowTimeService>();
        }

        [Fact]
        public void CreateShowtime_ValidData_ReturnsCreated()
        {
            // Arrange
            var movieData = new showResponse { Id = "Example-id", Title = "Example Movie"};
            _apiMock.Setup(api => api.GetMovieByIdAsync(It.IsAny<string>())).ReturnsAsync(movieData);

            var controller = new ShowtimeController(_apiMock.Object, _serviceMock.Object, _logMock.Object);

            // Act
            var result = controller.Create(new CreateShowtimeRequest { MovieId = "Example-id" }).Result;

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        //[Fact]
        //public void ReserveSeats_ValidData_ReturnsOK()
        //{
        //    // Arrange
        //    var seats = new List<SeatEntity>
        //    {
        //        new SeatEntity { Row = 1, SeatNumber = 1 },
        //        new SeatEntity { Row = 1, SeatNumber = 2 }
        //    };

        //    var showtime = new ShowtimeEntity { Id = 1, Movie = new MovieEntity(), SessionDate = DateTime.Now };

        //    var seatServiceMock = new Mock<ISeatService>();
        //    seatServiceMock.Setup(service => service.ReserveSeats(seats, showtime)).Returns(true);

        //    var controller = new ReservationController(null, null, seatServiceMock.Object);

        //    // Act
        //    var result = controller.ReserveSeats(seats, showtime.Id);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //    var okResult = (OkObjectResult)result;
        //    Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        //}

        //[Fact]
        //public void BuySeats_ValidData_ReturnsOK()
        //{
        //    // Arrange
        //    var reservationId = Guid.NewGuid();
        //    var seats = new List<SeatEntity>
        //    {
        //        new SeatEntity { Row = 1, SeatNumber = 1 },
        //        new SeatEntity { Row = 1, SeatNumber = 2 }
        //    };

        //    var reservationServiceMock = new Mock<IReservationService>();
        //    reservationServiceMock.Setup(service => service.IsReservationValid(reservationId)).Returns(true);
        //    reservationServiceMock.Setup(service => service.BuySeats(reservationId, seats)).Returns(true);

        //    var controller = new ReservationController(null, reservationServiceMock.Object);

        //    // Act
        //    var result = controller.BuySeats(reservationId, seats);

        //    // Assert
        //    Assert.IsType<OkResult>(result);
        //    var okResult = (OkResult)result;
        //    Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        //}
    }
}
