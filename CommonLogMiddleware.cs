using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace netcore
{
    // https://www.w3.org/Daemon/User/Config/Logging.html#common-logfile-format
    public class CommonLogMiddleware
    {
        private readonly RequestDelegate _next;

        public ILogger<CommonLogMiddleware> Logger { get; }

        public CommonLogMiddleware(RequestDelegate next, ILogger<CommonLogMiddleware> logger)
        {
            _next = next;
            Logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var rawTarget = httpContext.Features.Get<IHttpRequestFeature>().RawTarget;
            var originalBody = httpContext.Response.Body;
            var state = new LogState()
            {
                HttpContext = httpContext,
                ResponseStream = new CountingStream(httpContext.Response.Body),
                StartDate = DateTimeOffset.Now.ToString("d/MMM/yyyy:H:m:s zzz"),
                RemoteHost = httpContext.Connection.RemoteIpAddress.ToString(),
                Method = httpContext.Request.Method,
                RequestLine = $"{httpContext.Request.Method} {rawTarget} {httpContext.Request.Protocol}",
            };

            httpContext.Response.Body = state.ResponseStream;
            httpContext.Response.OnStarting(OnStarting, state);
            httpContext.Response.OnCompleted(OnCompleted, state);

            try
            {
                await _next(httpContext);
            }
            catch (Exception)
            {
                state.StatusCode = 500;
                throw;
            }
            finally
            {
                httpContext.Response.Body = originalBody;
            }
        }

        private Task OnStarting(object arg)
        {
            var state = (LogState)arg;
            var httpContext = state.HttpContext;

            state.StatusCode = httpContext.Response.StatusCode;
            // Authentication is run later in the pipeline so this may be our first chance to see the result
            state.AuthUser = httpContext.User.Identity.Name ?? "Anonymous";

            return Task.CompletedTask;
        }

        private Task OnCompleted(object arg)
        {
            var state = (LogState)arg;
            state.Bytes = state.ResponseStream.BytesWritten;
            state.TransactionCompleteDate = DateTimeOffset.Now.ToString("yyyy-MM-dd");

            WriteLog(state);
            return Task.CompletedTask;
        }

        private void WriteLog(LogState state)
        {
            // TODO: Date formating
            Logger.LogInformation($"{state.RemoteHost} {state.TransactionCompleteDate} {state.Method} {state.Rfc931} {state.AuthUser} [{state.StartDate}] \"{state.RequestLine}\" {state.StatusCode} {state.Bytes}");
        }

        private class LogState
        {
            public HttpContext HttpContext { get; set; }
            public CountingStream ResponseStream { get; set; }

            // Time taken for transaction to complete in seconds, field has type <fixed>
            public long TimeTaken { get; set; } 
            // Method, field has type <name>
            public string Method { get; set; }
            public string RemoteHost { get; set; } 
            public string Rfc931 { get; set; } = "Rfc931"; // ??
            public string AuthUser { get; set; }
            public string StartDate { get; set; }
            // Date at which transaction completed, field has type <date>
            public string TransactionCompleteDate { get; set; }
            // Time at which transaction completed, field has type <time>
            public string TransactionCompleteTime { get; set; }
            public string RequestLine { get; set; }
            // Status code, field has type <integer>
            public int StatusCode { get; set; }
            // bytes transferred, field has type <integer>
            public long Bytes { get; set; }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CommonLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseCommonLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CommonLogMiddleware>();
        }
    }
}
