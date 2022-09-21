using EasyAppInsights.Api.DI.Models;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace EasyAppInsights.Api.DI
{
    public class ApiTelemetryInitialiser : ITelemetryInitializer
    {
        readonly TelemetryInitOptions _telemetryInitOptions;

        public ApiTelemetryInitialiser(TelemetryInitOptions telemetryInitOptions)
        {
            _telemetryInitOptions = telemetryInitOptions;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var roleNameArray = new[] { _telemetryInitOptions.Environment, _telemetryInitOptions.AppName };
            var roleName = string.Join(":", roleNameArray.Where(x => !string.IsNullOrWhiteSpace(x)));
            telemetry.Context.Cloud.RoleName = roleName;
        }
    }
}
