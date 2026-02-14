# Blazor Web Interface Guide

## Overview

The Tournament Schedule Generator now includes a modern Blazor Server web interface for configuring tournament parameters and generating schedules through an intuitive, form-based UI.

## Getting Started

### Running the Web Application

```bash
cd src/ScheduleGenerator.Web
dotnet run
```

The application will start and be available at `http://localhost:5000` (or the port specified in launchSettings.json).

### Accessing the Application

1. Open your web browser
2. Navigate to `http://localhost:5000`
3. Click "Configure Tournament" or use the navigation menu to access the tournament configuration page

## Features

### Interactive Configuration

The web interface provides a comprehensive form-based configuration system with the following sections:

#### 1. Basic Information
- **Tournament Name**: Set a name for your tournament

#### 2. Teams Management
- Add/remove teams dynamically
- Assign optional seed values for team ranking
- Supports any number of teams

#### 3. Fields Configuration
- Define multiple playing fields
- Set availability windows for each field with start and end times
- Add multiple availability windows per field (e.g., different days)

#### 4. Tournament Format
Choose from three tournament formats:
- **Round Robin**: Every team plays every other team
- **Group Stage**: Teams divided into groups with optional advancement
  - Configure number of groups
  - Set teams advancing per group
- **Knockout**: Single or double elimination
  - Optional third-place match

#### 5. Scheduling Rules
Configure match timing and constraints:
- **Match Duration**: Length of each match in minutes
- **Buffer Between Matches**: Time between matches on the same field
- **Minimum Rest Time**: Required rest time for teams between matches
- **Max Matches Per Team Per Day**: Limit daily matches per team (optional)
- **Minimum Opponent Spacing**: Time between matches against the same opponent (optional)

#### 6. Constraint Weights (Optimization)
Fine-tune the scheduling algorithm by setting weights:
- **Balanced Kickoff Times**: Distribute start times evenly across teams
- **Minimize Field Changes**: Keep teams on the same field when possible
- **Opponent Spacing**: Maximize time between matches against the same opponent
- **Seeded Team Separation**: Ensure seeded teams don't meet early

Set weights to 0 to disable a constraint, or higher values to prioritize it.

### Actions

The Actions panel provides several utilities:

#### Generate Schedule
Click "Generate Schedule" to create an optimized tournament schedule based on your configuration. The system will:
1. Validate the configuration
2. Generate all required matches
3. Apply scheduling constraints and optimizations
4. Display the results

#### Validate Configuration
Check your configuration for errors before generating a schedule. This helps catch issues like:
- Insufficient field availability
- Invalid team counts for the selected format
- Missing required fields

#### Export Configuration (JSON)
Save your current configuration to a JSON file. This allows you to:
- Reuse configurations
- Share configurations with others
- Version control your tournament setups
- Use with the CLI tool

#### Import Configuration (JSON)
Load a previously saved configuration from a JSON file. Compatible with:
- Web interface exports
- Console application JSON files
- Manually created configuration files

#### Reset to Defaults
Clear all configuration and return to the default starting values.

### Schedule Display

Once a schedule is generated, it will be displayed in a table showing:
- Match ID
- Home team
- Away team
- Field assignment
- Start time
- End time

You can also download the complete schedule as a JSON file for further processing or record keeping.

## Configuration Validation

The system performs comprehensive validation including:
- Required fields check
- Team count validation for selected format
- Field availability verification
- Time window consistency
- Match duration feasibility
- Rest time requirements

Validation errors are displayed clearly with specific messages indicating what needs to be corrected.

## Tips for Best Results

1. **Field Availability**: Ensure you provide sufficient field availability for all matches. The system needs enough time slots to accommodate all games plus buffer times.

2. **Rest Times**: Set realistic rest times. Too long and you may not have enough time slots; too short and teams won't have adequate recovery.

3. **Constraint Weights**: Start with default weights (all 1) and adjust based on your priorities. Setting all weights high may make it harder to find a solution.

4. **Team Count**: For group stage tournaments, ensure team count is divisible by the number of groups for balanced groups.

5. **Match Duration**: Include any pre/post-match activities in the match duration or use buffer time appropriately.

## JSON Integration

The web interface is fully compatible with JSON configuration files used by the console application. You can:
- Create configurations in the web UI and export for use with the CLI
- Load CLI-created JSON files in the web interface
- Share configurations between team members
- Store configurations in version control

Example workflow:
1. Configure tournament in the web UI
2. Export to JSON
3. Commit JSON to version control
4. Use JSON with CLI in CI/CD pipelines

## Technical Details

### Architecture
- **Frontend**: Blazor Server with interactive components
- **Backend**: Same clean architecture layers as console app
  - Domain Layer: Core entities and constraints
  - Application Layer: Business logic and orchestration
  - Infrastructure Layer: Scheduling algorithms

### Technologies
- .NET 9
- Blazor Server (Interactive Server Components)
- Bootstrap 5 for styling
- Bootstrap Icons for UI elements

### Browser Compatibility
Modern browsers with WebSocket support:
- Chrome/Edge (recommended)
- Firefox
- Safari

## Troubleshooting

### Schedule Generation Takes Too Long
For complex tournaments (many teams, many constraints), schedule generation can take time. Consider:
- Reducing soft constraint weights
- Increasing field availability
- Reducing minimum rest times if too restrictive

### Validation Errors
Read error messages carefully - they indicate exactly what needs to be fixed. Common issues:
- Not enough time slots for all matches
- Team count incompatible with format (e.g., 5 teams for 2 groups)
- Overlapping time windows

### Import Fails
Ensure your JSON file:
- Is valid JSON (use a JSON validator)
- Follows the expected schema
- Has all required fields

## Support

For issues, questions, or contributions, please refer to the main repository documentation.
