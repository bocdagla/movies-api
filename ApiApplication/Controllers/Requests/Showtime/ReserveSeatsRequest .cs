using ApiApplication.Controllers.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Controllers.Requests.Showtime
{
    public class ReserveSeatsRequest
    {
        [Required]
        [ConsecutiveInts(ErrorMessage = "The Seats must be in consecutive order")]
        public IEnumerable<short> SeatIds { get; set; }

        [Required]
        public short Row { get; set; }
    }
}
