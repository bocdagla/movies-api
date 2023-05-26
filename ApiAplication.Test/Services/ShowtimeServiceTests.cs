using ApiApplication.ApiClients.Response;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApiApplication.Tests.Services
{
    public class ShowTimeServiceTests
    {
        private Mock<IAuditoriumsRepository> _auditoriumsRepositoryMock;
        private Mock<IShowtimesRepository> _showtimesRepositoryMock;
        private Mock<ITicketsRepository> _ticketsRepositoryMock;
        private ShowTimeService _showTimeService;

        public ShowTimeServiceTests()
        {
            _auditoriumsRepositoryMock = new Mock<IAuditoriumsRepository>();
            _showtimesRepositoryMock = new Mock<IShowtimesRepository>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _showTimeService = new ShowTimeService(_auditoriumsRepositoryMock.Object, _showtimesRepositoryMock.Object, _ticketsRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnShowtimeEntities()
        {
            // Arrange
            var expectedShowtimes = new List<ShowtimeEntity> { new ShowtimeEntity(), new ShowtimeEntity() };
            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync((s) => true, CancellationToken.None))
                .ReturnsAsync(expectedShowtimes);

            // Act
            var result = await _showTimeService.GetAsync();

            // Assert
            Assert.Equal(expectedShowtimes, result);
        }

        [Fact]
        public async Task GetAsync_WithShowtimeId_ShouldReturnShowtimeEntity()
        {
            // Arrange
            int showtimeId = 1;
            var expectedShowtime = new ShowtimeEntity();
            _showtimesRepositoryMock.Setup(repo => repo.GetWithMoviesByIdAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedShowtime);

            // Act
            var result = await _showTimeService.GetAsync(showtimeId);

            // Assert
            Assert.Equal(expectedShowtime, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedShowtimeEntity()
        {
            // Arrange
            int auditoriumId = 1;
            DateTime date = DateTime.Now;
            var movie = new MovieData();
            var expectedShowtime = new ShowtimeEntity();
            _showtimesRepositoryMock.Setup(repo => repo.CreateShowtime(It.IsAny<ShowtimeEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedShowtime);

            // Act
            var result = await _showTimeService.CreateAsync(auditoriumId, date, movie);

            // Assert
            Assert.Equal(expectedShowtime, result);
        }

        [Fact]
        public async Task AddReservationsAsync_ShouldReturnReservationId()
        {
            // Arrange
            CancellationToken cancellationToken = CancellationToken.None;
            int showtimeId = 1;
            short row = 1;
            var seatNumbers = new List<short> { 1, 2 };
            var showtime = new ShowtimeEntity();
            var seats = seatNumbers.Select(s => new SeatEntity { Row = row, SeatNumber = s });
            var ticket = new TicketEntity { Id = Guid.NewGuid() };

            _showtimesRepositoryMock.Setup(repo => repo.GetWithTicketsByIdAsync(showtimeId, cancellationToken))
                .ReturnsAsync(showtime);
            _ticketsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<ShowtimeEntity>(), It.IsAny<IEnumerable<SeatEntity>>(), cancellationToken))
                .ReturnsAsync(ticket);

            // Act
            var result = await _showTimeService.AddReservationsAsync(showtimeId, row, seatNumbers, cancellationToken);

            // Assert
            Assert.Equal(ticket.Id, result);
        }

        [Fact]
        public async Task ValidateSeatsAsync_WithValidSeats_ShouldReturnTrue()
        {
            // Arrange
            int showtimeId = 1;
            short row = 1;
            var seatNumbers = new List<short> { 1, 2 };
            var tickets = new List<TicketEntity> { new TicketEntity() { Seats = new List<SeatEntity>() } };
            var auditorium = new AuditoriumEntity() { Seats = new List<SeatEntity>() { 
                new SeatEntity() { Row = row, SeatNumber = 1 },
                new SeatEntity() { Row = row, SeatNumber = 2 } 
            }};
            _ticketsRepositoryMock.Setup(repo => repo.GetEnrichedAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tickets);
            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auditorium);

            // Act
            var result = await _showTimeService.ValidateSeatsAsync(showtimeId, row, seatNumbers);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateSeatsAsync_WithInvalidSeats_ShouldReturnFalse()
        {
            // Arrange
            int showtimeId = 1;
            short row = 1;
            var seatNumbers = new List<short> { 1, 2 };
            var tickets = new List<TicketEntity> { new TicketEntity() { Seats = new List<SeatEntity>() } };
            var auditorium = new AuditoriumEntity() { Seats = new List<SeatEntity>() };
            _ticketsRepositoryMock.Setup(repo => repo.GetEnrichedAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tickets);
            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auditorium);

            // Act
            var result = await _showTimeService.ValidateSeatsAsync(showtimeId, row, new List<short> { 3, 4 });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PurchaseReservation_WithValidReservationId_ShouldReturnTrue()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var ticket = new TicketEntity { Id = reservationId, CreatedTime = DateTime.Now };
            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(reservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _showTimeService.PurchaseReservation(reservationId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PurchaseReservation_WithInvalidReservationId_ShouldReturnFalse()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(reservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TicketEntity)null);

            // Act
            var result = await _showTimeService.PurchaseReservation(reservationId);

            // Assert
            Assert.False(result);
        }
    }
}
