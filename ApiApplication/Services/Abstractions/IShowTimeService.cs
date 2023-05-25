using ApiApplication.ApiClients.Response;
using ApiApplication.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Services.Abstractions
{
    public interface IShowTimeService
    {
        Task<IEnumerable<ShowtimeEntity>> GetAsync(CancellationToken cancellationToken = default);
        Task<ShowtimeEntity> GetAsync(int showtimeId, CancellationToken cancellationToken = default);
        Task<ShowtimeEntity> CreateAsync(int auditoriumId, DateTime date, MovieData movie, CancellationToken cancellationToken = default);
        Task<bool> ValidateSeatsAsync(int showtimeId, short row, IEnumerable<short> seatNumbers, CancellationToken cancellationToken = default);
        Task<Guid> AddReservationsAsync(int showtimeId, short row, IEnumerable<short> seatNumbers, CancellationToken cancellationToken = default);
        Task<bool> PurchaseReservation(Guid reservationId, CancellationToken cancellationToken = default);

    }
}
