using Streams;
using Uniscale.Core;
using Uniscale.Designtime;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(builder => {
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// Set up our session
var sessionTask = Platform.Builder()
    .WithInterceptors(i =>
        // Register the timeline functionality interceptor with access to the
        // forwarding session
        TimelineInterceptors.registerInterceptors(i))
    .Build();
// Acquire our Uniscale session
if (!sessionTask.IsCompleted)
    sessionTask.Wait();
var session = sessionTask.Result;

// Define the api endpoint we want to handle Uniscale features through
app.MapPost("/api/service-to-module/{featureId}", async (HttpRequest request, HttpResponse response) => {
    using (var reader = new StreamReader(request.Body)) {
        var content = await reader.ReadToEndAsync();
        // Handle the request by passing it the raw request body and return the result as json
        await response.WriteAsync((await session.AcceptGatewayRequest(content)).ToJson());
    }
});
app.Run();
