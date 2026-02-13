using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Console.UI;

namespace ScheduleGenerator.Console.Commands;

public class GenerateScheduleCommand : Command
{
    private readonly ITournamentOrchestrator _orchestrator;
    private readonly ILogger<GenerateScheduleCommand> _logger;

    public GenerateScheduleCommand(
        ITournamentOrchestrator orchestrator,
        ILogger<GenerateScheduleCommand> logger) 
        : base("generate", "Generate a tournament schedule through an interactive wizard")
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Add tournament name option
        var nameOption = new Option<string>(
            aliases: new[] { "--name", "-n" },
            description: "Tournament name",
            getDefaultValue: () => "Tournament");
        
        AddOption(nameOption);

        this.SetHandler(ExecuteAsync, nameOption);
    }

    private async Task ExecuteAsync(string tournamentName)
    {
        try
        {
            _logger.LogInformation("Starting tournament schedule generation wizard");
            
            System.Console.Clear();
            System.Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║           TOURNAMENT SCHEDULE GENERATOR - Interactive Wizard              ║");
            System.Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════╝");
            System.Console.WriteLine();
            System.Console.WriteLine($"Tournament: {tournamentName}");
            System.Console.WriteLine();

            // Step 1: Collect teams
            var teamCollector = new TeamCollector();
            var teams = teamCollector.CollectTeams();
            
            if (teams.Count < 2)
            {
                System.Console.WriteLine("\n❌ Error: At least 2 teams are required.");
                return;
            }

            // Step 2: Define fields
            var fieldCollector = new FieldCollector();
            var fields = fieldCollector.CollectFields();
            
            if (fields.Count < 1)
            {
                System.Console.WriteLine("\n❌ Error: At least 1 field is required.");
                return;
            }

            // Step 3: Set time windows
            var timeWindowCollector = new TimeWindowCollector();
            var (tournamentStart, tournamentEnd, blackouts) = timeWindowCollector.CollectTimeWindows();

            // Step 4: Select tournament format
            var formatSelector = new FormatSelector();
            var format = formatSelector.SelectFormat(teams.Count);

            // Step 5: Configure match rules
            var rulesConfigurator = new RulesConfigurator();
            var rules = rulesConfigurator.ConfigureRules();

            // Step 6: Configure constraints
            var constraintConfigurator = new ConstraintConfigurator();
            var constraints = constraintConfigurator.ConfigureConstraints();

            // Build tournament definition
            var definition = new TournamentDefinition
            {
                Name = tournamentName,
                Teams = teams,
                Fields = fields,
                Format = format,
                Rules = rules,
                Constraints = constraints
            };

            // Step 7: Execute scheduling
            System.Console.WriteLine("\n" + new string('=', 80));
            System.Console.WriteLine("Generating schedule...");
            System.Console.WriteLine("This may take a moment depending on the complexity of your tournament.");
            System.Console.WriteLine(new string('=', 80));

            var schedule = await _orchestrator.GenerateScheduleAsync(definition);

            // Step 8: Display results
            var outputRenderer = new OutputRenderer();
            outputRenderer.RenderSchedule(schedule);

            _logger.LogInformation("Schedule generation completed successfully");
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("\n\n⚠️  Schedule generation cancelled by user.");
            _logger.LogWarning("Schedule generation cancelled by user");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n\n❌ Error generating schedule: {ex.Message}");
            _logger.LogError(ex, "Error during schedule generation");
            throw;
        }
    }
}
