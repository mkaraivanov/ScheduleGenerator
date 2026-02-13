using FluentValidation;
using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Application.Validators;

/// <summary>
/// Validator for FormatConfiguration
/// </summary>
public class FormatConfigurationValidator : AbstractValidator<FormatConfiguration>
{
    public FormatConfigurationValidator()
    {
        RuleFor(f => f.Type)
            .NotEmpty()
            .WithMessage("Tournament format type is required.");

        RuleFor(f => f.Type)
            .Must(type => IsValidFormatType(type))
            .When(f => !string.IsNullOrWhiteSpace(f.Type))
            .WithMessage("Format type must be 'RoundRobin', 'Groups', or 'Knockout'.");

        RuleFor(f => f.GroupStage)
            .NotNull()
            .When(f => f.Type?.ToLowerInvariant() == "groups" || f.Type?.ToLowerInvariant() == "groupstage")
            .WithMessage("Group stage configuration is required for Groups format.");

        RuleFor(f => f.GroupStage)
            .SetValidator(new GroupStageConfigurationValidator()!)
            .When(f => f.GroupStage != null);
    }

    private bool IsValidFormatType(string type)
    {
        var normalizedType = type.ToLowerInvariant();
        return normalizedType == "roundrobin" ||
               normalizedType == "groups" ||
               normalizedType == "groupstage" ||
               normalizedType == "knockout";
    }
}

/// <summary>
/// Validator for GroupStageConfiguration
/// </summary>
public class GroupStageConfigurationValidator : AbstractValidator<GroupStageConfiguration>
{
    public GroupStageConfigurationValidator()
    {
        RuleFor(g => g.NumberOfGroups)
            .GreaterThan(0)
            .WithMessage("Number of groups must be greater than 0.");

        RuleFor(g => g.TeamsAdvancingPerGroup)
            .GreaterThan(0)
            .When(g => g.TeamsAdvancingPerGroup.HasValue)
            .WithMessage("Teams advancing per group must be greater than 0.");
    }
}
