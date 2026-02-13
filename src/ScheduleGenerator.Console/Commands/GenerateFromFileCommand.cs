using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Application.Validators;
using ScheduleGenerator.Console.UI;

namespace ScheduleGenerator.Console.Commands;

/// <summary>
/// Command to generate a tournament schedule from a JSON configuration file
/// </summary>
public class GenerateFromFileCommand : Command
{
    private readonly ITournamentOrchestrator _orchestrator;
    private readonly TournamentDefinitionValidator _validator;
    private readonly ILogger<GenerateFromFileCommand> _logger;

    public GenerateFromFileCommand(
        ITournamentOrchestrator orchestrator,
        TournamentDefinitionValidator validator,
        ILogger<GenerateFromFileCommand> logger)
        : base("generate-from-file", "Generate a tournament schedule from a JSON configuration file")
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Add config file option
        var configOption = new Option<FileInfo>(
            aliases: new[] { "--config", "-c" },
            description: "Path to the JSON configuration file")
        {
            IsRequired = true
        };
        configOption.AddValidator(result =>
        {
            var file = result.GetValueForOption(configOption);
            if (file != null && !file.Exists)
            {
                result.ErrorMessage = $"Configuration file not found: {file.FullName}";
            }
        });

        AddOption(configOption);

        // Add output file option
        var outputOption = new Option<FileInfo?>(
            aliases: new[] { "--output", "-o" },
            description: "Path to save the generated schedule (optional)");

        AddOption(outputOption);

        this.SetHandler(ExecuteAsync, configOption, outputOption);
    }

    private async Task ExecuteAsync(FileInfo configFile, FileInfo? outputFile)
    {
        try
        {
            _logger.LogInformation("Loading tournament configuration from {ConfigFile}", configFile.FullName);

            System.Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║        TOURNAMENT SCHEDULE GENERATOR - Configuration File Mode            ║");
            System.Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════╝");
            System.Console.WriteLine();
            System.Console.WriteLine($"📄 Loading configuration from: {configFile.FullName}");
            System.Console.WriteLine();

            // Read and deserialize JSON configuration
            TournamentDefinition definition;
            try
            {
                var jsonContent = await File.ReadAllTextAsync(configFile.FullName);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                
                definition = JsonSerializer.Deserialize<TournamentDefinition>(jsonContent, options)
                    ?? throw new InvalidOperationException("Failed to deserialize tournament definition");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON configuration file");
                System.Console.WriteLine($"❌ Error parsing JSON configuration: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read configuration file");
                System.Console.WriteLine($"❌ Error reading configuration file: {ex.Message}");
                return;
            }

            // Validate the configuration
            System.Console.WriteLine("✓ Configuration loaded successfully");
            System.Console.WriteLine($"  Tournament: {definition.Name}");
            System.Console.WriteLine($"  Teams: {definition.Teams.Count}");
            System.Console.WriteLine($"  Fields: {definition.Fields.Count}");
            System.Console.WriteLine($"  Format: {definition.Format.Type}");
            System.Console.WriteLine();

            System.Console.WriteLine("Validating configuration...");
            var validationResult = await _validator.ValidateAsync(definition);
            
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Configuration validation failed");
                System.Console.WriteLine("❌ Configuration validation failed:");
                System.Console.WriteLine();
                
                foreach (var error in validationResult.Errors)
                {
                    System.Console.WriteLine($"  • {error.PropertyName}: {error.ErrorMessage}");
                }
                return;
            }

            System.Console.WriteLine("✓ Configuration validated successfully");
            System.Console.WriteLine();

            // Execute scheduling
            System.Console.WriteLine(new string('=', 80));
            System.Console.WriteLine("Generating schedule...");
            System.Console.WriteLine("This may take a moment depending on the complexity of your tournament.");
            System.Console.WriteLine(new string('=', 80));
            System.Console.WriteLine();

            var schedule = await _orchestrator.GenerateScheduleAsync(definition);

            _logger.LogInformation("Schedule generated with {MatchCount} matches", schedule.Matches.Count);

            // Display the schedule using the renderer
            var renderer = new OutputRenderer();
            renderer.RenderSchedule(schedule);

            // Save to file if output path is specified
            if (outputFile != null)
            {
                await SaveScheduleToFile(schedule, outputFile);
            }
            else
            {
                System.Console.WriteLine();
                System.Console.WriteLine("💡 Tip: Use --output <file> to save the schedule to a JSON file");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during schedule generation");
            System.Console.WriteLine($"❌ Unexpected error: {ex.Message}");
        }
    }

    private async Task SaveScheduleToFile(ScheduleOutput schedule, FileInfo outputFile)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(schedule, options);
            await File.WriteAllTextAsync(outputFile.FullName, json);

            System.Console.WriteLine();
            System.Console.WriteLine($"✓ Schedule saved to: {outputFile.FullName}");
            
            _logger.LogInformation("Schedule saved to {OutputFile}", outputFile.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save schedule to file");
            System.Console.WriteLine($"❌ Failed to save schedule: {ex.Message}");
        }
    }
}
