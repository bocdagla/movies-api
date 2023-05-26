using ApiApplication.ApiClients.Response;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Services
{
    public class ShowTimeService : IShowTimeService
    {
        IAuditoriumsRepository _auditoriums;
        IShowtimesRepository _showtimes;
        ITicketsRepository _tickets;

        public ShowTimeService(IAuditoriumsRepository auditoriums, IShowtimesRepository showtimes, ITicketsRepository tickets)
        {
            _auditoriums = auditoriums;
            _showtimes = showtimes;
            _tickets = tickets;
        }

        public async Task<IEnumerable<ShowtimeEntity>> GetAsync(CancellationToken cancellationToken = default)
        {
            return await _showtimes.GetAllAsync((r) => true, cancellationToken);
        }

        public async Task<ShowtimeEntity> GetAsync(int showtimeId, CancellationToken cancellationToken = default)
        {
            return await _showtimes.GetWithMoviesByIdAsync(showtimeId, cancellationToken);
        }

        public async Task<ShowtimeEntity> CreateAsync(int auditoriumId, DateTime date, MovieData movie, CancellationToken cancellationToken = default)
        {
            return await _showtimes.CreateShowtime(new ShowtimeEntity
            {
                AuditoriumId = auditoriumId,
                SessionDate = date,
                Movie = Map(movie),
            }, cancellationToken);
        }


        public async Task<Guid> AddReservationsAsync(int showtimeId, short row, IEnumerable<short> seatNumbers, CancellationToken cancellationToken = default)
        {
            var showtime = await _showtimes.GetWithTicketsByIdAsync(showtimeId, cancellationToken);
            var seats = seatNumbers.Select(s => new SeatEntity()
            { 
                Row = row,
                SeatNumber = s
            });
            var ticket = await _tickets.CreateAsync(showtime, seats, cancellationToken);
            return ticket.Id;
        }


        public async Task<bool> ValidateSeatsAsync(int showtimeId, short row, IEnumerable<short> seatNumbers, CancellationToken cancellationToken = default)
        {
            // Checking if the seats requested are overlapping any ticket
            var tickets = await _tickets.GetEnrichedAsync(showtimeId, cancellationToken);
            var ticketValidationTask = Task.Run(() => {
                var ticketsWithRequestedSeats = tickets.Where(t => t.Seats.Any(s => s.Row == row && seatNumbers.Contains(s.SeatNumber)));
                return !ticketsWithRequestedSeats.Any(t => t.Paid || DateTime.Now <= t.CreatedTime.AddMinutes(10));
            });

            // Checking if the seats requested exist in the auditorium
            var auditorium = await _auditoriums.GetAsync(showtimeId, cancellationToken);
            var seatValidationTask = Task.Run(() => {
                return seatNumbers.All(sn => auditorium.Seats.Any(s => s.Row == row && s.SeatNumber == sn)); 
            });

            return await ticketValidationTask && await seatValidationTask;
        }

        public async Task<bool> PurchaseReservation(Guid reservationId, CancellationToken cancellationToken = default)
        {
            var ticket = await _tickets.GetAsync(reservationId, cancellationToken);
            if(ticket == null || !IsTicketReserved(ticket)) 
                return false;
            await _tickets.ConfirmPaymentAsync(ticket, cancellationToken);
            return true;
        }

        private bool IsTicketReserved(TicketEntity ticket)
        {
            return !ticket.Paid && DateTime.Now <= ticket.CreatedTime.AddMinutes(10);
        }

        //TODO Move this to a mapper
        private MovieEntity Map(MovieData data)
        {
            return new MovieEntity
            {
                ReleaseDate = data.Year,
                Title = data.FullTitle,
                Stars = string.Concat(System.Linq.Enumerable.Repeat("*", data.Rank)),
                ImdbId = data.ImdbRating
            };
        }
    }
}