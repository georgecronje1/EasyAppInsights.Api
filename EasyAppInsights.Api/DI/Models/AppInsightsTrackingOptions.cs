using EasyAppInsights.Api.DI.Models.RequestResponseTracking;
using EasyAppInsights.Api.Filters.RequestResponseTracking;

namespace EasyAppInsights.Api.DI.Models
{
    public class AppInsightsTrackingOptions
    {
        public SqlTrackingOptions SqlOptions { get; private set; }

        public RequestTrackingOptions RequestOptions { get; private set; }

        public ResponseTrackingOptions ResponseOptions { get; private set; }

        public class SqlTrackingOptions
        {
            public bool ShouldTrack { get; private set; }

            SqlTrackingOptions(bool shouldTrack)
            {
                ShouldTrack = shouldTrack;
            }

            public SqlTrackingOptions EnableSqlTracking()
            {
                ShouldTrack = true;

                return this;
            }

            static SqlTrackingOptions Make(bool shouldTrack)
            {
                return new SqlTrackingOptions(shouldTrack);
            }

            public static SqlTrackingOptions MakeDefault()
            {
                return Make(shouldTrack: false);
            }
        }

        public class RequestTrackingOptions
        {
            public bool ShouldRecordRequest { get; private set; }

            public List<RequestTrackingFilterItem> Filters { get; private set; }

            RequestTrackingOptions(bool shouldRecordRequest)
            {
                ShouldRecordRequest = shouldRecordRequest;
                Filters = new List<RequestTrackingFilterItem>();
            }

            public RequestTrackingOptions EnableRequestTracking()
            {
                ShouldRecordRequest = true;

                return this;
            }

            public RequestTrackingOptions AddRequestBodyProcessor<T>() where T : IRequestTrackingFilter
            {

                Filters.Add(new RequestTrackingFilterItem(typeof(T)));

                return this;
            }

            static RequestTrackingOptions Make(bool shouldRecordRequest)
            {
                return new RequestTrackingOptions(shouldRecordRequest: shouldRecordRequest);
            }

            public static RequestTrackingOptions MakeDefault()
            {
                return Make(false);
            }
        }

        public class ResponseTrackingOptions
        {
            public bool ShouldRecordResponse { get; private set; }

            public List<ResponseTrackingFilterItem> Filters { get; private set; }

            ResponseTrackingOptions(bool shouldRecordResponse)
            {
                ShouldRecordResponse = shouldRecordResponse;
                Filters = new List<ResponseTrackingFilterItem>();
            }

            public ResponseTrackingOptions EnableResponseTracking()
            {
                ShouldRecordResponse = true;

                return this;
            }

            public ResponseTrackingOptions AddResponseBodyProcesor<T>() where T : IResponseTrackingFilter
            {
                Filters.Add(new ResponseTrackingFilterItem(typeof(T)));

                return this;
            }

            static ResponseTrackingOptions Make(bool shouldRecordResponse)
            {
                return new ResponseTrackingOptions(shouldRecordResponse: shouldRecordResponse);
            }

            public static ResponseTrackingOptions MakeDefault()
            {
                return Make(false);
            }
        }

        AppInsightsTrackingOptions(SqlTrackingOptions sqlOptions, RequestTrackingOptions requestOptions, ResponseTrackingOptions responseOptions)
        {
            SqlOptions = sqlOptions;
            RequestOptions = requestOptions;
            ResponseOptions = responseOptions;
        }

        public static AppInsightsTrackingOptions MakeWithDefaults()
        {
            var sqlOptions = SqlTrackingOptions.MakeDefault();

            var requestTrackingOptions = RequestTrackingOptions.MakeDefault();

            var responseTrackingOptions = ResponseTrackingOptions.MakeDefault();

            return new AppInsightsTrackingOptions(sqlOptions, requestTrackingOptions, responseTrackingOptions);
        }

        public AppInsightsTrackingOptions EnableTrackSql()
        {
            SqlOptions.EnableSqlTracking();

            return this;
        }

        public AppInsightsTrackingOptions EnableRequestAndResponseTracking()
        {
            EnableRequestTracking();
            EnableResponseTracking();

            return this;
        }

        public AppInsightsTrackingOptions AddRequestBodyProcessor<T>() where T : IRequestTrackingFilter
        {
            RequestOptions.AddRequestBodyProcessor<T>();

            return this;
        }

        public AppInsightsTrackingOptions AddResponseBodyProcessor<T>() where T : IResponseTrackingFilter
        {
            ResponseOptions.AddResponseBodyProcesor<T>();

            return this;
        }

        public AppInsightsTrackingOptions EnableRequestTracking()
        {
            RequestOptions.EnableRequestTracking();

            return this;
        }

        public AppInsightsTrackingOptions EnableResponseTracking()
        {
            ResponseOptions.EnableResponseTracking();

            return this;
        }
    }
}
