using System;

namespace ApiApplication.Controllers.Responses
{
    public class GetShowtimeResponse
    {
        public int Id { get; set; }
        public string MovieTitle { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
