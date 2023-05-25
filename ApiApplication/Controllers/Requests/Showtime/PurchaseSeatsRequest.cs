using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Controllers.Requests.Showtime
{
    public class PurchaseSeatsRequest
    {
        [Required]
        public Guid ReservationId { get; set; }
    }
}
