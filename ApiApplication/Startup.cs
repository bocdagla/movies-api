using ApiApplication.ApiClients;
using ApiApplication.ApiClients.Abstractions;
using ApiApplication.Database;
using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Middleware;
using ApiApplication.Services;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoDefinitions;
using StackExchange.Redis;
using System;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace ApiApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Logger
            services.AddLogging();

            // Redis Add
            var multiplexer = ConnectionMultiplexer.Connect(Configuration["RedisURL"]);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddSingleton(cfg =>
            {
                IConnectionMultiplexer multiplexer = cfg.GetService<IConnectionMultiplexer>();
                return multiplexer.GetDatabase();
            });
            services.AddSingleton<ICacheService, CacheService>();


            // Repository adds
            services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
            services.AddTransient<ITicketsRepository, TicketsRepository>();
            services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();

            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            // Api adds
            services.AddGrpcClient<MoviesApi.MoviesApiClient>(o =>
            {
                o.Address = new Uri(Configuration["MovieApiUrl"]);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            services.AddTransient<IMovieApiClient, MovieApiClientGrpc>();

            // Service adds
            services.AddScoped<IShowTimeService, ShowTimeService>();

            services.AddControllers();

            services.AddHttpClient();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<TimingMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SampleData.Initialize(app);
        }
    }
}
