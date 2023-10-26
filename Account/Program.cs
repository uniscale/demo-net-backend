
using Uniscale.Designtime;
using UniscaleDemo.Account;
using UniscaleDemo.Account.Account;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(builder => {
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// Create in memory cache of users
var users = new Dictionary<Guid, UserFull>();

// Initialize the Uniscale session
var sessionTask = Platform.Builder()
    .WithInterceptors(i => i
        .InterceptRequest(
            Patterns.Account.GetOrRegister.AllRequestUsages,
            Patterns.Account.GetOrRegister.Handle((input, ctx) => {
                // Get the existing user if there is a match on user handle
                var existingUser = users.Values
                    .FirstOrDefault(u => u.Handle == input);
                if (existingUser != null)
                    return existingUser;

                // Create a new user and return it
                var newUserIdentifier = Guid.NewGuid();
                users.Add(newUserIdentifier, new UserFull {
                    UserIdentifier = newUserIdentifier,
                    Handle = input
                });
                return users[newUserIdentifier];
            })
        )
        .InterceptRequest(
            Patterns.Account.LookupUsers.AllRequestUsages,
            Patterns.Account.LookupUsers.Handle((input, ctx) => {
                return input
                    .Where(identifier => users.ContainsKey(identifier))
                    .Select(identifier => users[identifier])
                    .ToList();
            })
        )
        .InterceptRequest(
            Patterns.Account.SearchAllUsers.AllRequestUsages,
            Patterns.Account.SearchAllUsers.Handle((input, ctx) => {
                return users.Values 
                    .Where(u => u.Handle.ToLower().Contains(input.ToLower()))
                    .ToList();
            })
        ))
    .Build();
if (!sessionTask.IsCompleted)
    sessionTask.Wait();
var session = sessionTask.Result;

app.MapPost("/api/service-to-module/{featureId}", async (HttpRequest request, HttpResponse response) => {
    using (var reader = new StreamReader(request.Body)) {
        var content = await reader.ReadToEndAsync();
        await response.WriteAsync((await session.AcceptGatewayRequest(content)).ToJson());
    }
});
app.Run();
