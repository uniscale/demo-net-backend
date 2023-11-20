# Demo solution .NET backend

## How to run

Add nuget source for `Demo company`'s solution in Uniscale into `nuget.config`.

 - Then you can run in IDE of choice
 - or in command line:
   - `dotnet run --project Account`
   - `dotnet run --project Messages`

These will run each service on their own servers.

## How to use

Send backend action request to `/api/service-to-module/{featureId}`. Port 5298 for account service and port 5192 for messages.
