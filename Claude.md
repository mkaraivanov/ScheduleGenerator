# Tournament Schedule Generator

Modern .NET 9 console application for constraint-based tournament scheduling.

## Architecture

**Clean Architecture** with three-layer separation:
- **Domain**: Core entities, constraints, tournament formats (zero dependencies)
- **Application**: Use cases, orchestration, validation (depends on Domain)
- **Infrastructure**: Backtracking algorithm, slot generation (implements Application interfaces)
- **Console**: Interactive CLI with System.CommandLine

## Core Flow

```
Tournament Definition → Match Generation → Scheduling
     (input)              (who plays whom)    (when & where)
```

## Key Components

- **Constraint System**: Hard (must satisfy) + Soft (optimize) constraints via `IConstraint`
- **Tournament Formats**: Round-robin, Groups, Knockout via Strategy pattern
- **Scheduling Engine**: Backtracking with CSP heuristics (MRV, LCV, forward checking)
- **Slot Model**: Pre-generated `Field × Time` combinations

## Tech Stack

- .NET 9
- System.CommandLine (interactive wizard + JSON configuration)
- xUnit + FluentAssertions (TDD)
- FluentValidation (input validation)
- Serilog (structured logging)

## Usage Modes

1. **Interactive Wizard**: Step-by-step prompts for configuration
2. **JSON Configuration**: Load tournament settings from a JSON file (faster, repeatable)

## Project Structure

```
src/
├── ScheduleGenerator.Domain/        # Entities, constraints, formats
├── ScheduleGenerator.Application/   # Services, DTOs, validators
├── ScheduleGenerator.Infrastructure/ # Algorithms, slot generation
└── ScheduleGenerator.Console/       # CLI wizard

tests/
├── ScheduleGenerator.Domain.Tests/
├── ScheduleGenerator.Application.Tests/
└── ScheduleGenerator.Integration.Tests/
```

## Development Approach

**TDD**: Write tests before implementation
- Unit tests for each constraint, format, component
- Integration tests for end-to-end scenarios
- Performance tests with BenchmarkDotNet

## Running

```bash
# Build and test
dotnet build
dotnet test

# Interactive wizard mode
dotnet run --project src/ScheduleGenerator.Console

# JSON configuration mode
dotnet run --project src/ScheduleGenerator.Console -- generate-from-file -c tournament-config.json
```

See [JSON-Configuration.md](JSON-Configuration.md) for detailed JSON configuration guide.
