using FluentValidation;
using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Application.Validators;

/// <summary>
/// Validator for FieldDefinition
/// </summary>
public class FieldDefinitionValidator : AbstractValidator<FieldDefinition>
{
    public FieldDefinitionValidator()
    {
        RuleFor(f => f.Name)
            .NotEmpty()
            .WithMessage("Field name is required.");

        RuleFor(f => f.AvailabilityWindows)
            .NotEmpty()
            .WithMessage("At least one availability window is required for each field.");

        RuleForEach(f => f.AvailabilityWindows)
            .SetValidator(new TimeWindowValidator());

        RuleFor(f => f.AvailabilityWindows)
            .Must(windows => !HasOverlappingWindows(windows))
            .When(f => f.AvailabilityWindows != null && f.AvailabilityWindows.Any())
            .WithMessage("Field availability windows must not overlap.");
    }

    private bool HasOverlappingWindows(List<TimeWindow> windows)
    {
        for (int i = 0; i < windows.Count; i++)
        {
            for (int j = i + 1; j < windows.Count; j++)
            {
                if (windows[i].Start < windows[j].End && windows[i].End > windows[j].Start)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

/// <summary>
/// Validator for TimeWindow
/// </summary>
public class TimeWindowValidator : AbstractValidator<TimeWindow>
{
    public TimeWindowValidator()
    {
        RuleFor(w => w.End)
            .GreaterThan(w => w.Start)
            .WithMessage("End time must be after start time.");

        RuleFor(w => w)
            .Must(w => (w.End - w.Start).TotalMinutes >= 1)
            .WithMessage("Time window duration must be at least 1 minute.");
    }
}
