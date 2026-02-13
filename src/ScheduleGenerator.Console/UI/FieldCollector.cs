using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class FieldCollector
{
    public List<FieldDefinition> CollectFields()
    {
        var fields = new List<FieldDefinition>();
        
        System.Console.WriteLine("\n=== Field Collection ===");
        System.Console.WriteLine("Enter field information (press Enter without field ID to finish)");
        
        while (true)
        {
            System.Console.Write($"\nField {fields.Count + 1} ID (or press Enter to finish): ");
            var fieldId = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(fieldId))
            {
                if (fields.Count < 1)
                {
                    System.Console.WriteLine("⚠️  You need at least 1 field for a tournament.");
                    continue;
                }
                break;
            }
            
            var availabilityWindows = new List<TimeWindow>();
            
            System.Console.WriteLine($"\n  Define availability windows for field '{fieldId}':");
            while (true)
            {
                System.Console.Write($"  Window {availabilityWindows.Count + 1} - Start (yyyy-MM-dd HH:mm) or Enter to finish: ");
                var startInput = System.Console.ReadLine()?.Trim();
                
                if (string.IsNullOrWhiteSpace(startInput))
                {
                    if (availabilityWindows.Count < 1)
                    {
                        System.Console.WriteLine("    ⚠️  Each field needs at least 1 availability window.");
                        continue;
                    }
                    break;
                }
                
                if (!DateTime.TryParse(startInput, out var startTime))
                {
                    System.Console.WriteLine("    ⚠️  Invalid date format. Use: yyyy-MM-dd HH:mm");
                    continue;
                }
                
                System.Console.Write($"  Window {availabilityWindows.Count + 1} - End (yyyy-MM-dd HH:mm): ");
                var endInput = System.Console.ReadLine()?.Trim();
                
                if (!DateTime.TryParse(endInput, out var endTime))
                {
                    System.Console.WriteLine("    ⚠️  Invalid date format. Use: yyyy-MM-dd HH:mm");
                    continue;
                }
                
                if (endTime <= startTime)
                {
                    System.Console.WriteLine("    ⚠️  End time must be after start time.");
                    continue;
                }
                
                availabilityWindows.Add(new TimeWindow { Start = startTime, End = endTime });
                System.Console.WriteLine($"    ✓ Added window: {startTime:yyyy-MM-dd HH:mm} to {endTime:yyyy-MM-dd HH:mm}");
            }
            
            var field = new FieldDefinition
            {
                Name = fieldId,
                AvailabilityWindows = availabilityWindows
            };
            
            fields.Add(field);
            System.Console.WriteLine($"✓ Added field: {fieldId} with {availabilityWindows.Count} window(s)");
        }
        
        System.Console.WriteLine($"\n✓ Total fields: {fields.Count}");
        return fields;
    }
}
