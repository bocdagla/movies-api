namespace ApiApplication.ApiClients.Response
{
    public sealed class GetMovieResponse
    {
        public bool Success { get; set; }
        public MovieData[] Movies { get; set; }
        public MovieError[] Errors { get; set; }
    }
}
