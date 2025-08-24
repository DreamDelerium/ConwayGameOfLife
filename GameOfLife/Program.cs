using GameOfLife.Middleware;
using GameOfLife.Models;
using GameOfLife.RedisRepositories;
using GameOfLife.Services;
using GameOfLife.Validators;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");

// Configure Serilog
builder.Logging.ClearProviders();
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
        retainedFileCountLimit: 7
    )
);

Log.Logger.Information("Logger configured successfully");

// Register Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
    var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;

    try
    {
        var connection = ConnectionMultiplexer.Connect(redisConnectionString);
        logger.LogInformation("Successfully connected to Redis at {ConnectionString}", redisConnectionString);
        return connection;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to Redis at {ConnectionString}", redisConnectionString);
        throw; 
    }
});

builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("Redis"));

builder.Services.Configure<GameSettings>(
    builder.Configuration.GetSection("GameSettings"));

// Add controllers, CORS, Swagger, HTTP context accessor
services.AddControllers();
// Add application services
services.AddScoped<IBoardValidator, BoardValidator>();
services.AddScoped<IRedisBoardRepository, RedisBoardRepository>();
services.AddScoped<IGameOfLifeService, GameOfLifeService>();


services.AddHttpContextAccessor();
services.AddEndpointsApiExplorer();
services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conway's Game of Life API",
        Version = "v1",
        Description = "A RESTful API implementation of Conway's Game of Life",
        Contact = new OpenApiContact
        {
            Name = "Game of Life API",
            Email = "jasonwucinski@yahoo.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure middleware pipeline

// Use middleware first to catch all unhandled exceptions
app.UseMiddleware<ErrorHandlingMiddleware>();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conway's Game of Life API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Conway's Game of Life API";
});

// Redirect HTTP to HTTPS in non-development
if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowAll");

// Enable authorization (if needed)
app.UseAuthorization();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/", () => Results.Redirect("/index.html"));

// Map controller routes
app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();
