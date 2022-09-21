using EasyAppInsights.Api.DI.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace EasyAppInsights.Api.Middleware.RequestResponseTracking
{
    public class RequestBodyLoggingMiddleware : IMiddleware
    {
        readonly AppInsightsTrackingOptions.RequestTrackingOptions _requestTrackingOptions;

        public RequestBodyLoggingMiddleware(AppInsightsTrackingOptions.RequestTrackingOptions requestTrackingOptions)
        {
            _requestTrackingOptions = requestTrackingOptions ?? throw new System.ArgumentNullException(nameof(requestTrackingOptions), message: "You probably forgot to add services.AddAppInsightsTracking() in your Startup");
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_requestTrackingOptions.ShouldRecordRequest)
            {
                var method = context.Request.Method;

                // Ensure the request body can be read multiple times
                context.Request.EnableBuffering();

                // Only if we are dealing with POST or PUT, GET and others shouldn't have a body
                if (context.Request.Body.CanRead && (method == HttpMethods.Post || method == HttpMethods.Put))
                {
                    // Leave stream open so next middleware can read it
                    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 512, leaveOpen: true))
                    {
                        var requestBody = await reader.ReadToEndAsync();

                        // Reset stream position, so next middleware can read it
                        context.Request.Body.Position = 0;

                        var requestTelemetry = context.Features.Get<RequestTelemetry>();

                        if (_requestTrackingOptions.Filters.Any())
                        {
                            _requestTrackingOptions.Filters.ForEach(filter =>
                            {
                                var service = (Filters.RequestResponseTracking.IRequestTrackingFilter)context.RequestServices.GetService(filter.Filter);

                                var additionalProps = service.GetExtraTrackingProperties(context.Request.Path.Value, requestBody);

                                additionalProps.ForEach(additionalProp =>
                                {
                                    requestTelemetry?.Properties.Add(additionalProp.Name, additionalProp.Value);
                                });

                                requestBody = service.ProcessBody(context.Request.Path.Value, requestBody);

                            });
                        }

                        // Write request body to App Insights
                        requestTelemetry?.Properties.Add("RequestBody", requestBody);


                    }
                }
            }

            // Call next middleware in the pipeline
            await next(context);
        }
    }
}
