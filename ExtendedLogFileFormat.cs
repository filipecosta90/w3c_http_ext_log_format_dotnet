
// based on https://github.com/Tratcher/CommonLog

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace netcore
{
    public class ExtendedLogFileFormat
    {
        private readonly RequestDelegate _next;

        public ILogger<ExtendedLogFileFormat> Logger { get; }

        public ExtendedLogFileFormat(RequestDelegate next, ILogger<ExtendedLogFileFormat> logger)
        {
            _next = next;
            Logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var rawTarget = httpContext.Features.Get<IHttpRequestFeature>().RawTarget;
            var originalBody = httpContext.Response.Body;
            long ContentLength = 0;
             if (httpContext.Request.ContentLength.HasValue)
{
    ContentLength = httpContext.Request.ContentLength.Value;
}
                

            var state = new LogState()
            {
                TransactionStartDateTime = DateTime.UtcNow,
                HttpContext = httpContext,
                ResponseStream = new CountingStream(httpContext.Response.Body),
                S_IP = httpContext.Connection.LocalIpAddress.ToString(),
                S_Port = httpContext.Connection.LocalPort,
                C_IP = httpContext.Connection.RemoteIpAddress.ToString(),
                S_ComputerName = Environment.MachineName.ToString(),
                CS_Host = httpContext.Request.Host.ToString(),
                CS_Method = httpContext.Request.Method.ToString(),
                CS_Uri_Query = httpContext.Request.QueryString.ToString(),
                CS_Bytes = ContentLength,
                CS_User_Agent = httpContext.Request.Headers[HeaderNames.UserAgent],
                CS_Referer = httpContext.Request.Headers[HeaderNames.Referer],
                CS_Cookie = httpContext.Request.Headers[HeaderNames.Cookie],
               
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
                state.SC_Status = 500;
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
            state.SC_Status = state.HttpContext.Response.StatusCode;
            // Authentication is run later in the pipeline so this may be our first chance to see the result
            state.CS_Username = state.HttpContext.User.Identity.Name ?? "-";
            return Task.CompletedTask;
        }

        private Task OnCompleted(object arg)
        {
            var utcNow = DateTime.UtcNow;
            var state = (LogState)arg;
            //state.Bytes = state.ResponseStream.BytesWritten;
            state.TransactionCompleteDate = utcNow.ToString("yyyy-MM-dd");
            state.TransactionCompleteTime = utcNow.ToString("HH:mm:ss");
            state.Time_Taken = (utcNow - state.TransactionStartDateTime).TotalSeconds;
            WriteLog(state);
            return Task.CompletedTask;
        }

        private void WriteLog(LogState state)
        {
            // TODO: Date formating
            Logger.LogInformation($"{state.TransactionCompleteDate} {state.TransactionCompleteTime} {state.C_IP} {state.CS_Username} {state.S_Sitename} {state.S_ComputerName} {state.S_IP} {state.S_Port} {state.CS_Method} {state.CS_Uri_Stem} {state.CS_Uri_Query} {state.SC_Status} {state.SC_Win32_Status} {state.SC_Bytes} {state.CS_Bytes} {state.Time_Taken} {state.CS_Version} {state.CS_Host} {state.CS_User_Agent} {state.CS_Cookie} {state.CS_Referer} {state.XForwardedForHeader} ");
        }

        private class LogState
        {
            public HttpContext HttpContext { get; set; }
            public CountingStream ResponseStream { get; set; }

            // Date at which transaction completed, field has type <date>
            // All dates are specified in GMT. This format is chosen to assist collation using sort.
            public string TransactionCompleteDate { get; set; } = "-";
            public DateTime TransactionStartDateTime { get; set; }

            // Time at which transaction completed, field has type <time>
            // All times are specified in GMT
            public string TransactionCompleteTime { get; set; } = "-";

            // The IP address of the client that accessed your server.
            public string C_IP { get; set; } = "-";

            // The name of the authenticated user who accessed your server.
            // This does not include anonymous users, who are represented by a hyphen (-).
            public string CS_Username { get; set; } = "-";

            // The Internet service and instance number that was accessed by a client.
            public string S_Sitename { get; set; } = "-";

            // The name of the server on which the log entry was generated.
            public string S_ComputerName { get; set; } = "-";

            // The IP address of the server on which the log entry was generated.
            public string S_IP { get; set; } = "-";

            // The port number the client is connected to.
            public int S_Port { get; set; }

            // Method, field has type <name>
            // The action the client was trying to perform (for example, a GET method).
            public string CS_Method { get; set; } = "-";

            // The resource accessed; for example, Default.htm.
            public string CS_Uri_Stem { get; set; } = "-";

            // The query, if any, the client was trying to perform.
            public string CS_Uri_Query { get; set; } = "-";

            // The status of the action, in HTTP or FTP terms.
            // field has type <integer>
            public int SC_Status { get; set; }

            // The status of the action, in terms used by Microsoft Windows®.
            // field has type <integer>
            public int SC_Win32_Status { get; set; }

            // The number of bytes sent by the server.
            public long SC_Bytes { get; set; }

            // The number of bytes received by the server.
            public long CS_Bytes { get; set; }
    
            // Time taken for transaction to complete in seconds, field has type <fixed>
            public double Time_Taken { get; set; } 

            // The protocol (HTTP, FTP) version used by the client. For HTTP this will be either HTTP 1.0 or HTTP 1.1.
            public string CS_Version { get; set; } = "-";

            // Displays the content of the host header.
            public string CS_Host { get; set; } = "-";

            // The browser used on the client.
            public string CS_User_Agent { get; set; } = "-";

            // The content of the cookie sent or received, if any.
            public string CS_Cookie { get; set; } = "-";

            // The previous site visited by the user. This site provided a link to the current site.
            public string CS_Referer { get; set; } = "-";

            // bytes transferred, field has type <integer>
            public string XForwardedForHeader { get; set; } = "-";

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExtendedLogFileFormatExtensions
    {
        public static IApplicationBuilder UseExtendedLogFileFormat(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExtendedLogFileFormat>();
        }
    }
}
