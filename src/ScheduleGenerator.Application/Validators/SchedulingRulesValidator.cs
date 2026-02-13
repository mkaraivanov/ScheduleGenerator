using FluentValidation;
using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Application.Validators;

/// <summary>
/// Validator for SchedulingRules
/// </summary>
public class SchedulingRulesValidator : AbstractValidator<SchedulingRules>
{
    public SchedulingRulesValidator()
    {
        RuleFor(r => r.MatchDurationMinutes)
            .GreaterThan(0)
            .WithMessage("Match duration must be greater than 0 minutes.");

        RuleFor(r => r.MatchDurationMinutes)
            .LessThanOrEqualTo(600)
            .WithMessage("Match duration must not exceed 600 minutes (10 hours).");

        RuleFor(r => r.BufferBetweenMatchesMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Buffer time must be non-negative.");

        RuleFor(r => r.BufferBetweenMatchesMinutes)
            .LessThanOrEqualTo(120)
            .WithMessage("Buffer time must not exceed 120 minutes (2 hours).");

        RuleFor(r => r.MinimumRestTimeMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum rest time must be non-negative.");

        RuleFor(r => r.MaxMatchesPerTeamPerDay)
            .GreaterThan(0)
            .When(r => r.MaxMatchesPerTeamPerDay.HasValue)
            .WithMessage("Maximum matches per team per day must be greater than 0.");

        RuleFor(r => r.MinimumOpponentSpacingMinutes)
            .GreaterThanOrEqualTo(0)
            .When(r => r.MinimumOpponentSpacingMinutes.HasValue)
            .WithMessage("Minimum opponent spacing must be non-negative.");
    }
}
