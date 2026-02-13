using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class FormatSelector
{
    public FormatConfiguration SelectFormat(int teamCount)
    {
        System.Console.WriteLine("\n=== Tournament Format Selection ===");
        System.Console.WriteLine("Available formats:");
        System.Console.WriteLine("  1. Round Robin - Every team plays every other team");
        System.Console.WriteLine("  2. Groups - Teams divided into groups, round robin within groups");
        System.Console.WriteLine("  3. Knockout - Single elimination bracket");
        
        int choice;
        while (true)
        {
            System.Console.Write("\nSelect format (1-3): ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (int.TryParse(input, out choice) && choice >= 1 && choice <= 3)
            {
                break;
            }
            
            System.Console.WriteLine("⚠️  Please enter a number between 1 and 3.");
        }
        
        var config = new FormatConfiguration { Type = "" };
        
        switch (choice)
        {
            case 1:
                config = config with { Type = "RoundRobin" };
                break;
                
            case 2:
                int groupCount;
                while (true)
                {
                    System.Console.Write($"Number of groups (2-{teamCount / 2}): ");
                    var groupInput = System.Console.ReadLine()?.Trim();
                    
                    if (int.TryParse(groupInput, out groupCount) && groupCount >= 2 && groupCount <= teamCount / 2)
                    {
                        break;
                    }
                    
                    System.Console.WriteLine($"⚠️  Please enter a number between 2 and {teamCount / 2}.");
                }
                
                config = config with 
                { 
                    Type = "Groups",
                    GroupStage = new GroupStageConfiguration { NumberOfGroups = groupCount }
                };
                break;
                
            case 3:
                System.Console.Write("Include third place match? (y/n, default n): ");
                var thirdPlaceInput = System.Console.ReadLine()?.Trim().ToLowerInvariant();
                var includeThirdPlace = thirdPlaceInput == "y" || thirdPlaceInput == "yes";
                
                config = config with
                {
                    Type = "Knockout",
                    Knockout = new KnockoutConfiguration { IncludeThirdPlaceMatch = includeThirdPlace }
                };
                break;
        }
        
        System.Console.WriteLine($"\n✓ Selected format: {config.Type}");
        return config;
    }
}
