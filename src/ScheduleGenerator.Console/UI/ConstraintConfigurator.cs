using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class ConstraintConfigurator
{
    public ConstraintConfiguration ConfigureConstraints()
    {
        System.Console.WriteLine("\n=== Constraint Configuration ===");
        System.Console.WriteLine("Configure soft constraints (used for optimization)");
        
        int balancedKickoffWeight = 0;
        int minimizeFieldChangesWeight = 0;
        int opponentSpacingWeight = 0;
        int seededTeamSeparationWeight = 0;
        
        System.Console.WriteLine("\nAvailable constraints:");
        System.Console.WriteLine("  1. Balanced Kickoff Times - Ensure teams play at various times");
        System.Console.WriteLine("  2. Minimize Field Changes - Keep teams on same field when possible");
        System.Console.WriteLine("  3. Opponent Spacing - Maximize time between matches vs same opponent");
        System.Console.WriteLine("  4. Seeded Team Separation - Keep top-seeded teams apart early");
        
        System.Console.Write("\nEnable Balanced Kickoff Times constraint? (y/n, default n): ");
        if (AskYesNo())
        {
            balancedKickoffWeight = AskWeight("Balanced Kickoff Times");
        }
        
        System.Console.Write("Enable Minimize Field Changes constraint? (y/n, default n): ");
        if (AskYesNo())
        {
            minimizeFieldChangesWeight = AskWeight("Minimize Field Changes");
        }
        
        System.Console.Write("Enable Opponent Spacing constraint? (y/n, default n): ");
        if (AskYesNo())
        {
            opponentSpacingWeight = AskWeight("Opponent Spacing");
        }
        
        System.Console.Write("Enable Seeded Team Separation constraint? (y/n, default n): ");
        if (AskYesNo())
        {
            seededTeamSeparationWeight = AskWeight("Seeded Team Separation");
        }
        
        var config = new ConstraintConfiguration
        {
            BalancedKickoffTimesWeight = balancedKickoffWeight,
            MinimizeFieldChangesWeight = minimizeFieldChangesWeight,
            OpponentSpacingWeight = opponentSpacingWeight,
            SeededTeamSeparationWeight = seededTeamSeparationWeight
        };
        
        var enabledCount = new[] { balancedKickoffWeight, minimizeFieldChangesWeight, opponentSpacingWeight, seededTeamSeparationWeight }
            .Count(w => w > 0);
        
        if (enabledCount > 0)
        {
            System.Console.WriteLine($"\n✓ Enabled {enabledCount} constraint(s)");
        }
        else
        {
            System.Console.WriteLine("\n✓ No soft constraints enabled");
        }
        
        return config;
    }
    
    private bool AskYesNo()
    {
        var input = System.Console.ReadLine()?.Trim().ToLowerInvariant();
        return input == "y" || input == "yes";
    }
    
    private int AskWeight(string constraintName)
    {
        while (true)
        {
            System.Console.Write($"  Weight for {constraintName} (1-10, default 5): ");
            var input = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                return 5;
            }
            
            if (int.TryParse(input, out var weight) && weight >= 1 && weight <= 10)
            {
                return weight;
            }
            
            System.Console.WriteLine("    ⚠️  Please enter a number between 1 and 10.");
        }
    }
}
