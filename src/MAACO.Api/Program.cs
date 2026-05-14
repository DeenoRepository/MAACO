using MAACO.Api.Realtime;
using MAACO.Api.Services;
using FluentValidation;
using MAACO.Api.Middleware;
using MAACO.Core.Abstractions.Events;
using MAACO.Core.Domain.Events;
using MAACO.Infrastructure;
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
builder.Services.AddSingleton<ISettingsService, InMemorySettingsService>();
builder.Services.AddSingleton<IProjectPathValidator, ProjectPathValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
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
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

var connectionString = builder.Configuration.GetConnectionString("Maaco") ?? "Data Source=maaco.db";
builder.Services.AddMaacoPersistence(connectionString);
builder.Services.AddMaacoInfrastructure();

builder.Services.AddSingleton<IEventHandler<TaskCreatedEvent>, TaskCreatedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<WorkflowStartedEvent>, WorkflowStartedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<WorkflowStepStartedEvent>, StepStartedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<WorkflowStepCompletedEvent>, StepCompletedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<WorkflowStepFailedEvent>, StepFailedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<LogReceivedEvent>, LogReceivedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<ToolExecutionStartedEvent>, ToolExecutionStartedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<ToolExecutionCompletedEvent>, ToolExecutionCompletedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<ApprovalRequestedEvent>, ApprovalRequestedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<WorkflowCompletedEvent>, WorkflowCompletedSignalrHandler>();
builder.Services.AddSingleton<IEventHandler<WorkflowFailedEvent>, WorkflowFailedSignalrHandler>();

var app = builder.Build();
app.Services.UseMaacoInfrastructure();

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
app.MapHub<WorkflowHub>("/workflowHub");
app.Run();
