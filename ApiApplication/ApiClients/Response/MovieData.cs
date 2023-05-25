using System;

namespace ApiApplication.ApiClients.Response
{
    public class MovieData
    {
        public string Id { get; set; }
        public int Rank { get; set; }
        public string Title { get; set; }
        public string FullTitle { get; set; }
        public string ImdbRating { get; set; }
        public DateTime Year { get; set; }
    }
}