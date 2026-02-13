using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class TimeWindowCollector
{
    public (DateTime tournamentStart, DateTime tournamentEnd, List<TimeWindow> blackouts) CollectTimeWindows()
    {
        System.Console.WriteLine("\n=== Time Window Configuration ===");
        
        DateTime tournamentStart;
        while (true)
        {
            System.Console.Write("Tournament start date (yyyy-MM-dd HH:mm): ");
            var startInput = System.Console.ReadLine()?.Trim();
            
            if (DateTime.TryParse(startInput, out tournamentStart))
            {
                break;
            }
            
            System.Console.WriteLine("⚠️  Invalid date format. Use: yyyy-MM-dd HH:mm");
        }
        
        DateTime tournamentEnd;
        while (true)
        {
            System.Console.Write("Tournament end date (yyyy-MM-dd HH:mm): ");
            var endInput = System.Console.ReadLine()?.Trim();
            
            if (DateTime.TryParse(endInput, out tournamentEnd) && tournamentEnd > tournamentStart)
            {
                break;
            }
            
            System.Console.WriteLine("⚠️  Invalid date or end time must be after start time.");
        }
        
        var blackouts = new List<TimeWindow>();
        
        System.Console.WriteLine("\n  Define blackout periods (optional):");
        System.Console.WriteLine("  (Times when no matches can be scheduled)");
        
        while (true)
        {
            System.Console.Write($"  Blackout {blackouts.Count + 1} - Start (yyyy-MM-dd HH:mm) or Enter to skip: ");
            var startInput = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(startInput))
            {
                break;
            }
            
            if (!DateTime.TryParse(startInput, out var blackoutStart))
            {
                System.Console.WriteLine("    ⚠️  Invalid date format. Use: yyyy-MM-dd HH:mm");
                continue;
            }
            
            System.Console.Write($"  Blackout {blackouts.Count + 1} - End (yyyy-MM-dd HH:mm): ");
            var endInput = System.Console.ReadLine()?.Trim();
            
            if (!DateTime.TryParse(endInput, out var blackoutEnd) || blackoutEnd <= blackoutStart)
            {
                System.Console.WriteLine("    ⚠️  Invalid date or end time must be after start time.");
                continue;
            }
            
            blackouts.Add(new TimeWindow { Start = blackoutStart, End = blackoutEnd });
            System.Console.WriteLine($"    ✓ Added blackout: {blackoutStart:yyyy-MM-dd HH:mm} to {blackoutEnd:yyyy-MM-dd HH:mm}");
        }
        
        System.Console.WriteLine($"\n✓ Tournament window: {tournamentStart:yyyy-MM-dd HH:mm} to {tournamentEnd:yyyy-MM-dd HH:mm}");
        if (blackouts.Any())
        {
            System.Console.WriteLine($"✓ Blackout periods: {blackouts.Count}");
        }
        
        return (tournamentStart, tournamentEnd, blackouts);
    }
}
