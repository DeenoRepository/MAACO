using MAACO.Api.Middleware;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DesktopUi", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

var connectionString = builder.Configuration.GetConnectionString("Maaco") ?? "Data Source=maaco.db";
builder.Services.AddMaacoPersistence(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MaacoDbContext>();
    await db.Database.MigrateAsync();
    await DbSeed.InitializeAsync(db);
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("DesktopUi");
app.UseAuthorization();
app.MapControllers();
app.Run();

