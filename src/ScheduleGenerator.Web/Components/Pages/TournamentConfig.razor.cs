using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Application.Validators;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;
using Microsoft.JSInterop;

namespace ScheduleGenerator.Web.Components.Pages;

public partial class TournamentConfig
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    
    // Basic Information
    private string tournamentName = "My Tournament";

    // Teams
    private List<TeamModel> teams = new()
    {
        new TeamModel { Name = "Team A" },
        new TeamModel { Name = "Team B" },
        new TeamModel { Name = "Team C" },
        new TeamModel { Name = "Team D" }
    };

    // Fields
    private List<FieldModel> fields = new()
    {
        new FieldModel 
        { 
            Name = "Main Field",
            AvailabilityWindows = new List<TimeWindowModel>
            {
                CreateDefaultTimeWindow()
            }
        }
    };

    // Format Configuration
    private string formatType = "RoundRobin";
    private int numberOfGroups = 2;
    private int? teamsAdvancingPerGroup = null;
    private bool includeThirdPlaceMatch = false;

    // Scheduling Rules
    private int matchDurationMinutes = 60;
    private int bufferBetweenMatchesMinutes = 10;
    private int minimumRestTimeMinutes = 60;
    private int? maxMatchesPerTeamPerDay = null;
    private int? minimumOpponentSpacingMinutes = null;

    // Constraint Weights
    private int balancedKickoffTimesWeight = 1;
    private int minimizeFieldChangesWeight = 1;
    private int opponentSpacingWeight = 1;
    private int seededTeamSeparationWeight = 1;

    // State
    private bool isGenerating = false;
    private string? errorMessage = null;
    private string? successMessage = null;
    private List<FluentValidation.Results.ValidationFailure> validationErrors = new();
    private ScheduleOutput? generatedSchedule = null;

    private void AddTeam()
    {
        teams.Add(new TeamModel { Name = $"Team {(char)('A' + teams.Count)}" });
    }

    private void RemoveTeam(TeamModel team)
    {
        teams.Remove(team);
    }

    private void AddField()
    {
        fields.Add(new FieldModel 
        { 
            Name = $"Field {fields.Count + 1}",
            AvailabilityWindows = new List<TimeWindowModel>
            {
                CreateDefaultTimeWindow()
            }
        });
    }

    private void RemoveField(FieldModel field)
    {
        fields.Remove(field);
    }

    private void AddTimeWindow(FieldModel field)
    {
        field.AvailabilityWindows.Add(CreateDefaultTimeWindow());
    }

    private void RemoveTimeWindow(FieldModel field, TimeWindowModel window)
    {
        field.AvailabilityWindows.Remove(window);
    }

    private TournamentDefinition BuildTournamentDefinition()
    {
        var format = new FormatConfiguration
        {
            Type = formatType
        };

        if (formatType == "Groups")
        {
            format = format with
            {
                GroupStage = new GroupStageConfiguration
                {
                    NumberOfGroups = numberOfGroups,
                    TeamsAdvancingPerGroup = teamsAdvancingPerGroup
                }
            };
        }
        else if (formatType == "Knockout")
        {
            format = format with
            {
                Knockout = new KnockoutConfiguration
                {
                    IncludeThirdPlaceMatch = includeThirdPlaceMatch
                }
            };
        }

        return new TournamentDefinition
        {
            Name = tournamentName,
            Teams = teams.Select(t => new TeamDefinition 
            { 
                Name = t.Name, 
                Seed = t.Seed 
            }).ToList(),
            Fields = fields.Select(f => new FieldDefinition
            {
                Name = f.Name,
                AvailabilityWindows = f.AvailabilityWindows.Select(w => new TimeWindow
                {
                    Start = w.Start,
                    End = w.End
                }).ToList()
            }).ToList(),
            Format = format,
            Rules = new SchedulingRules
            {
                MatchDurationMinutes = matchDurationMinutes,
                BufferBetweenMatchesMinutes = bufferBetweenMatchesMinutes,
                MinimumRestTimeMinutes = minimumRestTimeMinutes,
                MaxMatchesPerTeamPerDay = maxMatchesPerTeamPerDay,
                MinimumOpponentSpacingMinutes = minimumOpponentSpacingMinutes
            },
            Constraints = new ConstraintConfiguration
            {
                BalancedKickoffTimesWeight = balancedKickoffTimesWeight,
                MinimizeFieldChangesWeight = minimizeFieldChangesWeight,
                OpponentSpacingWeight = opponentSpacingWeight,
                SeededTeamSeparationWeight = seededTeamSeparationWeight
            }
        };
    }

    private async Task ValidateConfiguration()
    {
        try
        {
            errorMessage = null;
            successMessage = null;
            validationErrors.Clear();

            var definition = BuildTournamentDefinition();
            var result = await Validator.ValidateAsync(definition);

            if (result.IsValid)
            {
                successMessage = "Configuration is valid!";
            }
            else
            {
                validationErrors = result.Errors.ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating configuration");
            errorMessage = $"Validation error: {ex.Message}";
        }
    }

    private async Task GenerateSchedule()
    {
        try
        {
            isGenerating = true;
            errorMessage = null;
            successMessage = null;
            validationErrors.Clear();
            generatedSchedule = null;

            var definition = BuildTournamentDefinition();
            
            // Validate first
            var validationResult = await Validator.ValidateAsync(definition);
            if (!validationResult.IsValid)
            {
                validationErrors = validationResult.Errors.ToList();
                return;
            }

            // Generate schedule
            Logger.LogInformation("Generating schedule for tournament: {TournamentName}", definition.Name);
            generatedSchedule = await Orchestrator.GenerateScheduleAsync(definition);
            successMessage = $"Schedule generated successfully with {generatedSchedule.Matches.Count} matches!";
            
            Logger.LogInformation("Schedule generated successfully with {MatchCount} matches", generatedSchedule.Matches.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating schedule");
            errorMessage = $"Error generating schedule: {ex.Message}";
        }
        finally
        {
            isGenerating = false;
        }
    }

    private async Task ExportConfiguration()
    {
        try
        {
            var definition = BuildTournamentDefinition();
            var json = JsonSerializer.Serialize(definition, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await DownloadFile($"{tournamentName.Replace(" ", "-")}-config.json", json, "application/json");
            successMessage = "Configuration exported successfully!";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting configuration");
            errorMessage = $"Error exporting configuration: {ex.Message}";
        }
    }

    private async Task ImportConfiguration(InputFileChangeEventArgs e)
    {
        try
        {
            errorMessage = null;
            successMessage = null;

            var file = e.File;
            if (file.Size > 1024 * 1024) // 1MB limit
            {
                errorMessage = "File is too large. Maximum size is 1MB.";
                return;
            }

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var definition = JsonSerializer.Deserialize<TournamentDefinition>(json, options);
            if (definition == null)
            {
                errorMessage = "Failed to parse configuration file.";
                return;
            }

            // Load the configuration
            LoadFromDefinition(definition);
            successMessage = "Configuration imported successfully!";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error importing configuration");
            errorMessage = $"Error importing configuration: {ex.Message}";
        }
    }

    private void LoadFromDefinition(TournamentDefinition definition)
    {
        tournamentName = definition.Name;
        
        teams = definition.Teams.Select(t => new TeamModel 
        { 
            Name = t.Name, 
            Seed = t.Seed 
        }).ToList();
        
        fields = definition.Fields.Select(f => new FieldModel
        {
            Name = f.Name,
            AvailabilityWindows = f.AvailabilityWindows.Select(w => new TimeWindowModel
            {
                Start = w.Start,
                End = w.End
            }).ToList()
        }).ToList();
        
        formatType = definition.Format.Type;
        if (definition.Format.GroupStage != null)
        {
            numberOfGroups = definition.Format.GroupStage.NumberOfGroups;
            teamsAdvancingPerGroup = definition.Format.GroupStage.TeamsAdvancingPerGroup;
        }
        if (definition.Format.Knockout != null)
        {
            includeThirdPlaceMatch = definition.Format.Knockout.IncludeThirdPlaceMatch;
        }
        
        matchDurationMinutes = definition.Rules.MatchDurationMinutes;
        bufferBetweenMatchesMinutes = definition.Rules.BufferBetweenMatchesMinutes;
        minimumRestTimeMinutes = definition.Rules.MinimumRestTimeMinutes;
        maxMatchesPerTeamPerDay = definition.Rules.MaxMatchesPerTeamPerDay;
        minimumOpponentSpacingMinutes = definition.Rules.MinimumOpponentSpacingMinutes;
        
        if (definition.Constraints != null)
        {
            balancedKickoffTimesWeight = definition.Constraints.BalancedKickoffTimesWeight;
            minimizeFieldChangesWeight = definition.Constraints.MinimizeFieldChangesWeight;
            opponentSpacingWeight = definition.Constraints.OpponentSpacingWeight;
            seededTeamSeparationWeight = definition.Constraints.SeededTeamSeparationWeight;
        }
    }

    private async Task DownloadSchedule()
    {
        if (generatedSchedule == null) return;

        try
        {
            var json = JsonSerializer.Serialize(generatedSchedule, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await DownloadFile($"{generatedSchedule.TournamentName.Replace(" ", "-")}-schedule.json", json, "application/json");
            successMessage = "Schedule downloaded successfully!";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading schedule");
            errorMessage = $"Error downloading schedule: {ex.Message}";
        }
    }

    private async Task DownloadFile(string filename, string content, string contentType)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var base64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("downloadFile", filename, base64, contentType);
    }

    private void ResetConfiguration()
    {
        tournamentName = "My Tournament";
        teams = new List<TeamModel>
        {
            new TeamModel { Name = "Team A" },
            new TeamModel { Name = "Team B" },
            new TeamModel { Name = "Team C" },
            new TeamModel { Name = "Team D" }
        };
        fields = new List<FieldModel>
        {
            new FieldModel 
            { 
                Name = "Main Field",
                AvailabilityWindows = new List<TimeWindowModel>
                {
                    CreateDefaultTimeWindow()
                }
            }
        };
        formatType = "RoundRobin";
        numberOfGroups = 2;
        teamsAdvancingPerGroup = null;
        includeThirdPlaceMatch = false;
        matchDurationMinutes = 60;
        bufferBetweenMatchesMinutes = 10;
        minimumRestTimeMinutes = 60;
        maxMatchesPerTeamPerDay = null;
        minimumOpponentSpacingMinutes = null;
        balancedKickoffTimesWeight = 1;
        minimizeFieldChangesWeight = 1;
        opponentSpacingWeight = 1;
        seededTeamSeparationWeight = 1;
        generatedSchedule = null;
        errorMessage = null;
        successMessage = "Configuration reset to defaults.";
        validationErrors.Clear();
    }

    private void ClearValidationErrors()
    {
        validationErrors.Clear();
    }

    // Helper models for binding
    private class TeamModel
    {
        public string Name { get; set; } = string.Empty;
        public int? Seed { get; set; }
    }

    private class FieldModel
    {
        public string Name { get; set; } = string.Empty;
        public List<TimeWindowModel> AvailabilityWindows { get; set; } = new();
    }

    private class TimeWindowModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    // Helper method for creating default time windows
    private static TimeWindowModel CreateDefaultTimeWindow()
    {
        return new TimeWindowModel
        {
            Start = DateTime.Today.AddDays(1).AddHours(9),
            End = DateTime.Today.AddDays(1).AddHours(17)
        };
    }
}
