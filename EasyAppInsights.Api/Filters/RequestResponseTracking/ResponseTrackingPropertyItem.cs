namespace EasyAppInsights.Api.Filters.RequestResponseTracking
{
    public class ResponseTrackingPropertyItem
    {
        public string Name { get; }

        public string Value { get; }

        public ResponseTrackingPropertyItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
