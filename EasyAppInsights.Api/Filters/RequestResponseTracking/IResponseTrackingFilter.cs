namespace EasyAppInsights.Api.Filters.RequestResponseTracking
{
    public interface IResponseTrackingFilter
    {
        string ProcessBody(string path, string responseBody);
        List<ResponseTrackingPropertyItem> GetExtraTrackingProperties(string path, string responseBody);
    }
}
