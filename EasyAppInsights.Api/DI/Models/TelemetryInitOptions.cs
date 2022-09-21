namespace EasyAppInsights.Api.DI.Models
{
    public class TelemetryInitOptions
    {
        public string AppName { get; }

        public string Environment { get; }

        TelemetryInitOptions(string appName, string environment)
        {
            AppName = appName;
            Environment = environment;
        }

        public static TelemetryInitOptions Make(string appName, string environment)
        {
            return new TelemetryInitOptions(appName, environment);
        }
    }
}