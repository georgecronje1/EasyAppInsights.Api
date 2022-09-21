using EasyAppInsights.Api.DI.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace EasyAppInsights.Api.Middleware.RequestResponseTracking
{
    public class ResponseBodyLoggingMiddleware : IMiddleware
    {
        readonly AppInsightsTrackingOptions.ResponseTrackingOptions _responseTrackingOptions;

        public ResponseBodyLoggingMiddleware(AppInsightsTrackingOptions.ResponseTrackingOptions responseTrackingOptions)
        {
            _responseTrackingOptions = responseTrackingOptions ?? throw new System.ArgumentNullException(nameof(responseTrackingOptions), message: "You probably forgot to add services.AddAppInsightsTracking() in your Startup");
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var originalBodyStream = context.Response.Body;

            try
            {
                // Swap out stream with one that is buffered and suports seeking
                using (var memoryStream = new MemoryStream())
                {

                    context.Response.Body = memoryStream;

                    // hand over to the next middleware and wait for the call to return
                    await next(context);

                    if (_responseTrackingOptions.ShouldRecordResponse)
                    {
                        // Read response body from memory stream
                        memoryStream.Position = 0;
                        var reader = new StreamReader(memoryStream);
                        var responseBody = await reader.ReadToEndAsync();

                        // Copy body back to so its available to the user agent
                        memoryStream.Position = 0;
                        await memoryStream.CopyToAsync(originalBodyStream);

                        // Write response body to App Insights
                        var requestTelemetry = context.Features.Get<RequestTelemetry>();

                        if (_responseTrackingOptions.Filters.Any())
                        {
                            _responseTrackingOptions.Filters.ForEach(filter =>
                            {
                                var service = (Filters.RequestResponseTracking.IResponseTrackingFilter)context.RequestServices.GetService(filter.Filter);

                                var additionalProps = service.GetExtraTrackingProperties(context.Request.Path.Value, responseBody);

                                additionalProps.ForEach(additionalProp =>
                                {
                                    requestTelemetry?.Properties.Add(additionalProp.Name, additionalProp.Value);
                                });

                                responseBody = service.ProcessBody(context.Request.Path.Value, responseBody);
                            });
                        }


                        requestTelemetry?.Properties.Add("ResponseBody", responseBody);
                    }
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
