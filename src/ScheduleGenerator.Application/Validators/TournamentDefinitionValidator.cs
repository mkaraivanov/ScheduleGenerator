using FluentValidation;
using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Application.Validators;

/// <summary>
/// Validator for TournamentDefinition
/// </summary>
public class TournamentDefinitionValidator : AbstractValidator<TournamentDefinition>
{
    public TournamentDefinitionValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty()
            .WithMessage("Tournament name is required.");

        RuleFor(t => t.Teams)
            .NotEmpty()
            .WithMessage("At least one team is required.");

        RuleFor(t => t.Teams)
            .Must(teams => teams.Select(t => t.Name).Distinct().Count() == teams.Count)
            .When(t => t.Teams != null && t.Teams.Any())
            .WithMessage("Team names must be unique.");

        RuleFor(t => t.Fields)
            .NotEmpty()
            .WithMessage("At least one field is required.");

        RuleForEach(t => t.Fields)
            .SetValidator(new FieldDefinitionValidator());

        RuleFor(t => t.Format)
            .NotNull()
            .WithMessage("Tournament format is required.");

        RuleFor(t => t.Format)
            .SetValidator(new FormatConfigurationValidator());

        RuleFor(t => t.Rules)
            .NotNull()
            .WithMessage("Scheduling rules are required.");

        RuleFor(t => t.Rules)
            .SetValidator(new SchedulingRulesValidator());

        // Validate format-specific requirements
        RuleFor(t => t)
            .Must(t => ValidateFormatRequirements(t))
            .WithMessage(t => GetFormatValidationMessage(t));
    }

    private bool ValidateFormatRequirements(TournamentDefinition definition)
    {
        if (definition.Format?.Type == null || definition.Teams == null)
            return true;

        return definition.Format.Type.ToLowerInvariant() switch
        {
            "roundrobin" => definition.Teams.Count >= 2,
            "groups" or "groupstage" => ValidateGroupStageRequirements(definition),
            "knockout" => definition.Teams.Count >= 2 && IsPowerOfTwo(definition.Teams.Count),
            _ => true
        };
    }

    private bool ValidateGroupStageRequirements(TournamentDefinition definition)
    {
        if (definition.Format.GroupStage == null)
            return false;

        var numberOfGroups = definition.Format.GroupStage.NumberOfGroups;
        var teamCount = definition.Teams.Count;

        // Each group must have at least 2 teams
        return teamCount >= numberOfGroups * 2;
    }

    private bool IsPowerOfTwo(int n)
    {
        return n > 0 && (n & (n - 1)) == 0;
    }

    private string GetFormatValidationMessage(TournamentDefinition definition)
    {
        if (definition.Format?.Type == null)
            return "Invalid format configuration.";

        return definition.Format.Type.ToLowerInvariant() switch
        {
            "roundrobin" => "Round-robin format requires at least 2 teams.",
            "groups" or "groupstage" => "Group stage format requires at least 2 teams per group.",
            "knockout" => "Knockout format requires a power-of-2 number of teams (2, 4, 8, 16, etc.).",
            _ => $"Unknown format type: {definition.Format.Type}"
        };
    }
}
