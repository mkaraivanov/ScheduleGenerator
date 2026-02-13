using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class RulesConfigurator
{
    public SchedulingRules ConfigureRules()
    {
        System.Console.WriteLine("\n=== Match Rules Configuration ===");
        
        int matchDuration;
        while (true)
        {
            System.Console.Write("Match duration in minutes (default 60): ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                matchDuration = 60;
                break;
            }
            
            if (int.TryParse(input, out matchDuration) && matchDuration > 0)
            {
                break;
            }
            
            System.Console.WriteLine("⚠️  Please enter a positive number.");
        }
        
        int bufferTime;
        while (true)
        {
            System.Console.Write("Buffer time between matches in minutes (default 10): ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                bufferTime = 10;
                break;
            }
            
            if (int.TryParse(input, out bufferTime) && bufferTime >= 0)
            {
                break;
            }
            
            System.Console.WriteLine("⚠️  Please enter a non-negative number.");
        }
        
        int? restTime = null;
        while (true)
        {
            System.Console.Write("Minimum rest time between team matches in minutes (optional, press Enter to skip): ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }
            
            if (int.TryParse(input, out var rest) && rest > 0)
            {
                restTime = rest;
                break;
            }
            
            System.Console.WriteLine("⚠️  Please enter a positive number.");
        }
        
        int? maxMatchesPerDay = null;
        while (true)
        {
            System.Console.Write("Maximum matches per team per day (optional, press Enter to skip): ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }
            
            if (int.TryParse(input, out var max) && max > 0)
            {
                maxMatchesPerDay = max;
                break;
            }
            
            System.Console.WriteLine("⚠️  Please enter a positive number.");
        }
        
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = matchDuration,
            BufferBetweenMatchesMinutes = bufferTime,
            MinimumRestTimeMinutes = restTime ?? 0,
            MaxMatchesPerTeamPerDay = maxMatchesPerDay
        };
        
        System.Console.WriteLine($"\n✓ Match duration: {matchDuration} minutes");
        System.Console.WriteLine($"✓ Buffer time: {bufferTime} minutes");
        if (restTime.HasValue)
        {
            System.Console.WriteLine($"✓ Rest time: {restTime} minutes");
        }
        if (maxMatchesPerDay.HasValue)
        {
            System.Console.WriteLine($"✓ Max matches per day: {maxMatchesPerDay}");
        }
        
        return rules;
    }
}
