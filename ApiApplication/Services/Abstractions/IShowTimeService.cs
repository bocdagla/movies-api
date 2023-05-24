using ApiApplication.ApiClients.Response;
using System;

namespace ApiApplication.Services.Abstractions
{
    public interface IShowTimeService
    {
        void Create(int auditoriumId, DateTime date, GetMovieResponse movie);
    }
}
