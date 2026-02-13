using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ScheduleGenerator.Application.Interfaces;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Application.Validators;
using ScheduleGenerator.Console.Commands;
using ScheduleGenerator.Infrastructure.Algorithms;

// Set up configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Tournament Schedule Generator");

    // Build host with dependency injection
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // Register Application services
            services.AddScoped<IMatchGenerationService, MatchGenerationService>();
            services.AddScoped<ISchedulingService, SchedulingService>();
            services.AddScoped<ISlotGenerationService, SlotGenerationService>();
            services.AddScoped<ITournamentOrchestrator, TournamentOrchestrator>();

            // Register Infrastructure services
            services.AddScoped<ISchedulingEngine, BacktrackingScheduler>();

            // Register validators
            services.AddScoped<TournamentDefinitionValidator>();
            services.AddScoped<FieldDefinitionValidator>();
            services.AddScoped<FormatConfigurationValidator>();
            services.AddScoped<SchedulingRulesValidator>();

            // Register commands
            services.AddScoped<GenerateScheduleCommand>();
            services.AddScoped<GenerateFromFileCommand>();
        })
        .UseSerilog()
        .Build();

    // Create root command
    var rootCommand = new RootCommand("Tournament Schedule Generator - Create optimized tournament schedules")
    {
        ActivatorUtilities.CreateInstance<GenerateScheduleCommand>(host.Services),
        ActivatorUtilities.CreateInstance<GenerateFromFileCommand>(host.Services)
    };

    // Execute command
    return await rootCommand.InvokeAsync(args);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
