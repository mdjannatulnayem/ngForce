using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services

Log.Logger = new LoggerConfiguration().MinimumLevel.Fatal()
    .WriteTo.File("Logs/Engine-diary-.txt",rollingInterval:RollingInterval.Hour).CreateLogger();

builder.Host.UseSerilog();

// Configure CORS policy
builder.Services.AddCors(option =>
    option.AddPolicy("corspolicy", build => {
        build.AllowAnyOrigin().AllowAnyHeader();
    }));

var app = builder.Build();

// Configure the HTTP request pipeline

//------------ Currently none! ------------//

app.MapGet("ngforce/about",() => "ngForce v 1.1.0");

app.MapGet("ngforce/engine/{force:double}", (double force,ILogger<Program> _logger) =>
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
        _logger.Log(LogLevel.Critical,stringBuilder.ToString());
    }
    return StatusCodes.Status201Created;
});

app.MapGet("ngforce/engine", () =>
{
    int count = ngForce_DataModel.EngineForce.Count;
    int block = ngForce_DataModel.BlockSize;
    return count <= block ? 
        ngForce_DataModel.EngineForce.ToArray() : 
        ngForce_DataModel.EngineForce.Skip(count - block).ToArray<double>();
});

app.MapGet("ngForce/engine/last", () => {
    return ngForce_DataModel.EngineForce.LastOrDefault<double>();
});

app.MapGet("ngforce/ignition", () => { return ngForce_DataModel.Ignition; });

app.MapGet("ngforce/ignition/1", () => { 
    ngForce_DataModel.Ignition = true;
    return StatusCodes.Status201Created;
});

app.MapGet("ngforce/ignition/0", () => {
    ngForce_DataModel.Ignition = false;
    return StatusCodes.Status201Created;
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