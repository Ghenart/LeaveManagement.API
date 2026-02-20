using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // ILogger için gerekli
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger; // Sektör standardı: ILogger enjeksiyonu

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Manuel dosya yazmak yerine Serilog'u (ILogger üzerinden) kullanıyoruz.
                // Serilog bunu hem konsola hem de belirttiğimiz dosyaya güvenlice yazar.
                _logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);

                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Sunucu hatası oluştu.",
                Detail = exception.Message // Sektör standardı: Canlı ortamda (Production) bu Detail gizlenmelidir.
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}