using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ApiApplication.Middleware
{
    public class TimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TimingMiddleware> _logger;

        public TimingMiddleware(RequestDelegate next, ILogger<TimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"STARTED for: {context.Request.Method}:{context.Request.Path} - {DateTime.Now} \n");
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"ENDED for: {context.Request.Method}:{context.Request.Path} - {DateTime.Now} - Elapsed time: {stopwatch.ElapsedMilliseconds} ms \\n");
            }
        }
    }
}
