namespace EasyAppInsights.Api.Filters.RequestResponseTracking
{
    public interface IRequestTrackingFilter
    {
        string ProcessBody(string path, string requestBody);
        List<RequestTrackingPropertyItem> GetExtraTrackingProperties(string path, string requestBody);
    }
}
