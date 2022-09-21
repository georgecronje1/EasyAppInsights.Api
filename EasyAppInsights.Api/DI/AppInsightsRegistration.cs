using EasyAppInsights.Api.DI.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAppInsights.Api.DI
{
    public static class AppInsightsRegistration
    {
        /// <summary>
        /// Make sure to add the Instrumentation Key to AppSettings.
        /// <![CDATA[
        /// {
        ///  "Logging": {
        ///    "LogLevel": {
        ///      "Default": "Information",
        ///      "Microsoft.AspNetCore": "Information"
        ///    },
        ///    "ApplicationInsights": {
        ///     "LogLevel": {
        ///         "Default": "Information"
        ///     }
        ///    },
        ///  },
        ///  "AllowedHosts": "*",
        ///  "ApplicationInsights": {
        ///    "ConnectionString": "Copy connection string from Application Insights Resource Overview"
        ///  }
        ///}
        /// ]]>
        /// </summary>
        /// <returns></returns>
        public static AppInsightsRegistrationBuilder AddAppInsightsTracking(this IServiceCollection services)
        {
            var builder = new AppInsightsRegistrationBuilder(services);

            builder.Services.AddApplicationInsightsTelemetry();

            return builder;
        }

        public static AppInsightsRegistrationBuilder AddTelemetryInitOptions(this AppInsightsRegistrationBuilder builder, System.Func<TelemetryInitOptions> getOptions)
        {
            var options = getOptions();

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<ITelemetryInitializer, ApiTelemetryInitialiser>();

            return builder;
        }

        /// <summary>
        /// Remember to also add app.UseAppInsightsRequestResponseTracking()
        /// </summary>
        public static AppInsightsRegistrationBuilder AddAppInsightsTracking(this AppInsightsRegistrationBuilder builder, Action<AppInsightsTrackingOptions> getOptions)
        {
            var options = AppInsightsTrackingOptions.MakeWithDefaults();
            getOptions(options);
            builder.Services.AddSingleton(options);

            builder
                .AddSqlTracking(options.SqlOptions)
                .AddRequestTracking(options.RequestOptions)
                .AddResponseTracking(options.ResponseOptions);


            return builder;
        }


    }
}
