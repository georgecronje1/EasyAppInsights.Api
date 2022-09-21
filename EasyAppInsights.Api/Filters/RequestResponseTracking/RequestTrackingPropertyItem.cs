namespace EasyAppInsights.Api.Filters.RequestResponseTracking
{
    public class RequestTrackingPropertyItem
    {
        public string Name { get; }

        public string Value { get; }

        public RequestTrackingPropertyItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
