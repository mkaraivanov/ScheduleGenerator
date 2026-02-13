using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class OutputRenderer
{
    public void RenderSchedule(ScheduleOutput schedule)
    {
        System.Console.WriteLine("\n" + new string('=', 80));
        System.Console.WriteLine("TOURNAMENT SCHEDULE");
        System.Console.WriteLine(new string('=', 80));
        
        if (!schedule.Matches.Any())
        {
            System.Console.WriteLine("\n⚠️  No matches were scheduled.");
            return;
        }
        
        // Group matches by date
        var matchesByDate = schedule.Matches
            .GroupBy(m => m.StartTime.Date)
            .OrderBy(g => g.Key);
        
        foreach (var dateGroup in matchesByDate)
        {
            System.Console.WriteLine($"\n📅 {dateGroup.Key:dddd, MMMM dd, yyyy}");
            System.Console.WriteLine(new string('-', 80));
            
            var matchesByTime = dateGroup.OrderBy(m => m.StartTime).ThenBy(m => m.Field);
            
            foreach (var match in matchesByTime)
            {
                var startTime = match.StartTime.ToString("HH:mm");
                var endTime = match.EndTime.ToString("HH:mm");
                var field = match.Field.PadRight(10);
                var homeTeam = match.HomeTeam.PadRight(20);
                var awayTeam = match.AwayTeam.PadRight(20);
                
                System.Console.WriteLine($"  {startTime}-{endTime}  |  {field}  |  {homeTeam} vs {awayTeam}");
            }
        }
        
        System.Console.WriteLine("\n" + new string('=', 80));
        System.Console.WriteLine("STATISTICS");
        System.Console.WriteLine(new string('=', 80));
        System.Console.WriteLine($"Total matches: {schedule.Diagnostics.TotalMatches}");
        
        var totalDays = matchesByDate.Count();
        System.Console.WriteLine($"Tournament days: {totalDays}");
        
        var fields = schedule.Matches.Select(m => m.Field).Distinct().Count();
        System.Console.WriteLine($"Fields used: {fields}");
        
        var tournamentStart = schedule.Matches.Min(m => m.StartTime);
        var tournamentEnd = schedule.Matches.Max(m => m.EndTime);
        System.Console.WriteLine($"Duration: {tournamentStart:yyyy-MM-dd HH:mm} to {tournamentEnd:yyyy-MM-dd HH:mm}");
        
        if (schedule.Diagnostics.GenerationTime.HasValue)
        {
            System.Console.WriteLine($"Generation time: {schedule.Diagnostics.GenerationTime.Value.TotalSeconds:F2}s");
        }
        
        // Display diagnostics
        System.Console.WriteLine("\n" + new string('=', 80));
        System.Console.WriteLine("DIAGNOSTICS");
        System.Console.WriteLine(new string('=', 80));
        
        System.Console.WriteLine($"Schedule Valid: {(schedule.Diagnostics.IsValid ? "✓ Yes" : "❌ No")}");
        System.Console.WriteLine($"Hard Constraint Violations: {schedule.Diagnostics.HardConstraintViolations}");
        System.Console.WriteLine($"Soft Constraint Violations: {schedule.Diagnostics.SoftConstraintViolations}");
        
        if (schedule.Diagnostics.Violations.Any())
        {
            System.Console.WriteLine("\nViolations:");
            foreach (var violation in schedule.Diagnostics.Violations)
            {
                System.Console.WriteLine($"  ❌ {violation}");
            }
        }
        
        if (schedule.Diagnostics.Warnings.Any())
        {
            System.Console.WriteLine("\nWarnings:");
            foreach (var warning in schedule.Diagnostics.Warnings)
            {
                System.Console.WriteLine($"  ⚠️  {warning}");
            }
        }
        
        System.Console.WriteLine("\n" + new string('=', 80));
    }
}
