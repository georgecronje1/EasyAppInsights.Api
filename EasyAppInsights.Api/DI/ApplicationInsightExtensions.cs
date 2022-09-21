using EasyAppInsights.Api.Middleware.RequestResponseTracking;
using Microsoft.AspNetCore.Builder;

namespace EasyAppInsights.Api.DI
{
    public static class ApplicationInsightExtensions
    {
        /// <summary>
        /// You MUST add services.AddAppInsightsTracking() in your DI to use this!!!
        /// Should be placed BELOW UseRouting and UseAuth but ABOVE UseEndpoints
        /// </summary>
        public static IApplicationBuilder UseAppInsightsRequestResponseTracking(this IApplicationBuilder builder)
        {
            return builder
                .UseRequestBodyLogging()
                .UseResponseBodyLogging();
        }
        static IApplicationBuilder UseRequestBodyLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestBodyLoggingMiddleware>();
        }

        static IApplicationBuilder UseResponseBodyLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseBodyLoggingMiddleware>();
        }
    }
}
