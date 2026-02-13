using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Console.UI;

public class TeamCollector
{
    public List<TeamDefinition> CollectTeams()
    {
        var teams = new List<TeamDefinition>();
        
        System.Console.WriteLine("\n=== Team Collection ===");
        System.Console.WriteLine("Enter team information (press Enter without team name to finish)");
        
        while (true)
        {
            System.Console.Write($"\nTeam {teams.Count + 1} name (or press Enter to finish): ");
            var teamName = System.Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(teamName))
            {
                if (teams.Count < 2)
                {
                    System.Console.WriteLine("⚠️  You need at least 2 teams for a tournament.");
                    continue;
                }
                break;
            }
            
            System.Console.Write("  Club (optional): ");
            var club = System.Console.ReadLine()?.Trim();
            
            System.Console.Write("  Seed (optional, press Enter for auto): ");
            var seedInput = System.Console.ReadLine()?.Trim();
            int? seed = null;
            if (!string.IsNullOrWhiteSpace(seedInput) && int.TryParse(seedInput, out var parsedSeed))
            {
                seed = parsedSeed;
            }
            
            var team = new TeamDefinition
            {
                Name = teamName,
                Seed = seed ?? teams.Count + 1
            };
            
            teams.Add(team);
            System.Console.WriteLine($"✓ Added team: {teamName}");
        }
        
        System.Console.WriteLine($"\n✓ Total teams: {teams.Count}");
        return teams;
    }
}
