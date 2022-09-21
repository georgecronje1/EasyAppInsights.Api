# EasyAppInsights.Api
A wrapper that encapsulates some of the common App Insights functionality in an easy to use way

#Before you begin

Make sure to remove any and all old ApplicationInsights stuff you might have like the nuget package and the `TelemetryInitializer`

#Basic Setup
After adding the nuget package to your project, at a minimum you need to set up the dependency injection. 
In your `Startup.cs` add the following
```
using Psg.Core.ApplicationInsights.DI;
...
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    ...
    services.AddAppInsightsTracking();
```

And in your AppSettings add:
```
"ApplicationInsights": {
    "InstrumentationKey": "YOUR_KEY_HERE"
  }
```

This is the bare minimum setup. It will allow your application to begin sending basic telemetry and logs of `WARNING` severity or higher to application insights. 

If you also wish to send `Information` level logs to App Insights (recommended) add the `ApplicationInsights.LogLevel` section to the `Logging` section in your app AppSettings
See below:

```
"Logging": {
"IncludeScopes": false,
"LogLevel": {
    "Default": "Debug",
    "System": "Information",
    "Microsoft": "Information"
},
"ApplicationInsights": {
    "LogLevel": {
    "Default": "Information",
    "Microsoft": "Information",
    "Microsoft.Hosting.Lifetime": "Information"
    }
}
}
```
#Identifying your application
By Default ApplicationInsights tries to identify your application (telemetry coming from your application) and split it our from others. 
But it is best to explicitly identify both your application name and the environment. 
To do this, change the DI code in `Startup.cs` to the following:
```
services
    .AddAppInsightsTracking()
    .AddTelemetryInitOptions(() =>
    {
        var enc = "TEST";// YOU SHOULD PULL THIS FROM THE CONFIG e.g. Configuration.GetSection("Environment").Value;
        return TelemetryInitOptions.Make(appName: "THE APP NAME", enc.ToString());
    });
```

#Track ALL THE THINGS

ApplicationInsights can track a bunch of other telemetry of your app. 
Things like the Request/Response body and the SQL being sent to the database.
There are two parts to enabling this. (all in `Startup.cs`)
1) Set up the DI
2) Set up the Request/Response middleware

```
using Psg.Core.ApplicationInsights.DI;
...
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    ...
    services
        .AddAppInsightsTracking()
        .AddTelemetryInitOptions(() =>
        {
            var enc = "TEST";// YOU SHOULD PULL THIS FROM THE CONFIG e.g. Configuration.GetSection("Environment").Value;
            return TelemetryInitOptions.Make(appName: "THE APP NAME", enc.ToString());
        })
        .AddAppInsightsTracking(options =>
        {
            options
                .EnableTrackSql()
                .EnableRequestTracking()
                .EnableResponseTracking(); //// you can also just call this.EnableRequestAndResponseTracking();
                //NOTE: in an app like PDF Generator you probably don't want to track the Response (BIG byte array)
        });
        ...
        ...
}
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    ...
    app.UseRouting();
    app.UseAdfsAuth();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("../swagger/v1/swagger.json", "Exilir.WelcomePacks.Api V1");
    });
    
    // ADD THIS LINE HERE
    // It MUST be BEFORE .UseEndpoints() and AFTER .UseRouting()
    app.UseAppInsightsRequestResponseTracking();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    ...
}
```

#Configuring what to send
As mentioned above, you can enable your app to send the full request and response body to ApplicationInsights. 
This is very handy BUT also not ideal in the case of very large payloads or even things like entire documents. 
It is also not super precise, sometimes you may what to extract specific bits of information (like a reference number or identifier) from the request and include that in the telemetry. 

This can be achieved with Filters. 

You have two types of filters (RequestFilter and ResponseFilter). The operate in virtually the same way.
To make use of them there are two steps:
1. Create a new class that implements either `IRequestTrackingFilter` or `IResponseTrackingFilter` (you can have multiple filters of each type)
2. Include those classes in your Startup (will show below)

Example:
```
public class AppInsightsRequestFilter : IRequestTrackingFilter
{
    public string ProcessBody(string path, string requestBody)
    {
        if (path.Contains("api/Data/SendData"))
        {
            requestBody = Regex.Replace(requestBody, "\"DataItem1\":\"[^\"]*\"", "\"DataItem1\":\"*******\"");
        }
        return requestBody; // NOTE: YOU MUST return the requestBody here, if you do not... an empty string will be sent to ApplicationInsights (and passed on to the next filter)
    }
    // NOTE: Any changes to the responseBody here does not affect what is passed to your app ONLY what is sent to ApplicationInsights
    public List<RequestTrackingPropertyItem> GetExtraTrackingProperties(string path, string requestBody)
    {
        if (path.Contains("api/Data/SendData"))
        {
            var dataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<IncomingData>(requestBody);
            return new List<RequestTrackingPropertyItem>()
            {
                new RequestTrackingPropertyItem("DataItem2", dataItems.DataItem2)
            };
        }
        return new List<RequestTrackingPropertyItem>();
    }
    public class IncomingData
    {
        public string DataItem1 { get; set; }
        public string DataItem2 { get; set; }
    }
}
public class AppInsightsResponseFilter : IResponseTrackingFilter
{
    public string ProcessBody(string path, string responseBody)
    {
        responseBody = Regex.Replace(responseBody, "\"dataItem1\":\"[^\"]*\"", "\"dataItem1\":\"*******\"");
        return responseBody;
    }
    public List<ResponseTrackingPropertyItem> GetExtraTrackingProperties(string path, string responseBody)
    {
        return new List<ResponseTrackingPropertyItem>();
    }
}
```

The above are some examples of how you can hide some data in the requestBody and responseBody that will be sent to ApplicationInsights
It also shows how you can extract some properties from that data (on a given endpoint) and include that in the telemetry.

To actually make use of these filters, add that as shown below in your `Startup.cs`
Mostly the same as above, but note the changes after `.EnableResponseTracking()`
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    ...
    services
        .AddAppInsightsTracking()
        .AddTelemetryInitOptions(() =>
        {
            var enc = "TEST";// YOU SHOULD PULL THIS FROM THE CONFIG e.g. Configuration.GetSection("Environment").Value;
            return TelemetryInitOptions.Make(appName: "THE APP NAME", enc.ToString());
        })
        .AddAppInsightsTracking(options =>
        {
            options
                .EnableTrackSql()
                .EnableRequestTracking()
                .EnableResponseTracking()//And right here is where the magic happens
                .AddRequestBodyProcessor<AppInsightsRequestFilter>()
                .AddResponseBodyProcessor<AppInsightsResponseFilter>();
        });
        ...
        ...
}
```
