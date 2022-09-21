using EasyAppInsights.Api.Middleware.RequestResponseTracking;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAppInsights.Api.DI.Models
{
    public class AppInsightsRegistrationBuilder
    {
        public IServiceCollection Services;

        public AppInsightsRegistrationBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public AppInsightsRegistrationBuilder AddSqlTracking(AppInsightsTrackingOptions.SqlTrackingOptions options)
        {
            Services.AddSingleton(options);
            Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = options.ShouldTrack;
            });

            return this;
        }

        public AppInsightsRegistrationBuilder AddRequestTracking(AppInsightsTrackingOptions.RequestTrackingOptions options)
        {
            Services.AddSingleton(options);
            Services.AddTransient<RequestBodyLoggingMiddleware>();

            options.Filters.ForEach(filter =>
            {
                Services.AddTransient(filter.Filter);
            });

            return this;
        }

        public AppInsightsRegistrationBuilder AddResponseTracking(AppInsightsTrackingOptions.ResponseTrackingOptions options)
        {
            Services.AddTransient<ResponseBodyLoggingMiddleware>();
            Services.AddSingleton(options);

            options.Filters.ForEach(filter =>
            {
                Services.AddTransient(filter.Filter);
            });

            return this;
        }
    }
}
