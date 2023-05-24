using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Controllers.Requests
{
    public class CreateShowtimeRequest
    {
        [Required]
        public int AuditoriumId { get; set; }

        [Required, MinLength(1)]
        public string MovieId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"AuditoriumId: {AuditoriumId}, MovieId: {MovieId}, Date: {Date}";
        }
    }
}
