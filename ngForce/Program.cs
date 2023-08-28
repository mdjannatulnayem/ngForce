using ngForce.Authentication;

using Serilog;
using Serilog.Events;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose() // Set the minimum level to the lowest
                            // allowing filtering
    .WriteTo.File("Logs/Engine-diary-.txt", rollingInterval: RollingInterval.Hour)
    .Filter.ByIncludingOnly(evt =>
    {
        // Include only log events with the desired log level and from your own code
        return evt.Level >= LogEventLevel.Information && ngForceCustomLogEvent(evt);
    })
    .CreateLogger();

// Define a method to check if a log event originates from your code
bool ngForceCustomLogEvent(LogEvent evt)
{
    // Check if the log event message contains the custom marker
    return evt.RenderMessage().Contains("//------------ READING ------------//");
}

builder.Host.UseSerilog();

// Configure CORS policy
builder.Services.AddCors(option =>
    option.AddPolicy("corspolicy", build => {
        build.AllowAnyOrigin().AllowAnyHeader();
    }));

var app = builder.Build();

// Configure the HTTP request pipeline

//------------ Currently none! ------------//

var g = app.MapGroup("g1").AddEndpointFilter<ApiEndpointAuthFilter>();

g.MapGet("ngforce/about", () => Results.Ok("ngForce v 1.1.0"));

g.MapGet("ngforce/engine/{force:double}", (double force, ILogger<Program> _logger) =>
{
    ngForce_DataModel.EngineForce.Add(force);
    int block = ngForce_DataModel.BlockSize;
    var count = ngForce_DataModel.EngineForce.Count;
    if (count > 0 && count % block == 0)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("//------------ READING ------------//");
        foreach (double f in ngForce_DataModel.EngineForce.Skip(count - block))
        {
            stringBuilder.AppendLine($"{f}");
        }
        _logger.Log(LogLevel.Information, stringBuilder.ToString());
    }
    return Results.StatusCode(StatusCodes.Status201Created);
});

g.MapGet("ngforce/engine", () =>
{
    int count = ngForce_DataModel.EngineForce.Count;
    int block = ngForce_DataModel.BlockSize;
    return count <= block ? Results.Ok(ngForce_DataModel.EngineForce.ToArray<double>())
        : Results.Ok(ngForce_DataModel.EngineForce.Skip(count - block).ToArray<double>());
});

g.MapGet("ngforce/engine/last", () => {
    return Results.Ok(ngForce_DataModel.EngineForce.LastOrDefault<double>());
});

g.MapGet("ngforce/ignition", () => { return Results.Ok(ngForce_DataModel.Ignition); });

g.MapGet("ngforce/ignition/1", () => {
    ngForce_DataModel.Ignition = true;
    return Results.StatusCode(StatusCodes.Status201Created);
});

g.MapGet("ngforce/ignition/0", () => {
    ngForce_DataModel.Ignition = false;
    return Results.StatusCode(StatusCodes.Status201Created);
});

var url = builder.Configuration.GetSection("Urls").GetValue<string>("Any");

// URL in the appsettings
if (!string.IsNullOrEmpty(url))
{
    app.Urls.Add(url);
}

app.UseCors("corspolicy");

app.Run();


static class ngForce_DataModel
{
    public static bool Ignition { get; set; } = false;
    public static int BlockSize { get; set; } = 100;
    public static List<double> EngineForce { get; set; } = new();
}