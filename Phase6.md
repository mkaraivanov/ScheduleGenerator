# Phase 6: Infrastructure & Configuration

**Goal**: Production-ready configuration, logging, and dependency injection setup.

## Tasks

### 6.1 Configuration Files
**Location**: `src/ScheduleGenerator.Console/`

Create `appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Ansi",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "Scheduling": {
    "MaxBacktrackingDepth": 10000,
    "TimeoutSeconds": 300,
    "EnableForwardChecking": true
  },
  "Constraints": {
    "BalancedKickoffs": {
      "Enabled": true,
      "Weight": 10
    },
    "MinimizeFieldChanges": {
      "Enabled": true,
      "Weight": 15
    },
    "OpponentSpacing": {
      "Enabled": false,
      "Weight": 5
    }
  }
}
```

Create `appsettings.Development.json`:
- More verbose logging (Debug level)
- Lower timeout for faster testing
- Different constraint weights for experimentation

### 6.2 Strongly-Typed Configuration
**Location**: `src/ScheduleGenerator.Console/Configuration/`

Create configuration classes:
- `SchedulingOptions`: max depth, timeout, algorithm settings
- `ConstraintOptions`: dictionary of constraint configs with weights
- Use `IOptions<T>` pattern throughout
- Validate configuration on startup with `ValidateOnStart()`

### 6.3 Dependency Injection Registration
**Location**: `src/ScheduleGenerator.Console/DependencyInjection/`

Create extension methods:
- `AddDomain(this IServiceCollection)`: register format generators
- `AddApplication(this IServiceCollection)`: register services, validators
- `AddInfrastructure(this IServiceCollection)`: register scheduling engine, constraint evaluator
- `AddValidators(this IServiceCollection)`: register FluentValidation validators

Service lifetimes:
- **Transient**: Validators, format generators (stateless)
- **Scoped**: Services (per-tournament scope)
- **Singleton**: Configuration, logging

Register constraints:
- Use `IEnumerable<IConstraint>` for constraint collection
- Dynamically discover and register all constraint implementations
- Allow configuration to enable/disable individual constraints

### 6.4 Logging Integration
**Location**: Throughout all services

Inject `ILogger<T>` into:
- All services (Application layer)
- Scheduling engine (Infrastructure)
- Constraint evaluator
- Format generators

Log structured events:
- **Debug**: Algorithm steps, variable selections, slot attempts
- **Information**: Match generation started/completed, scheduling started/completed
- **Warning**: Soft constraint violations, performance warnings
- **Error**: Hard constraint violations, scheduling failures, exceptions

Structured properties:
- `{TournamentId}`, `{MatchCount}`, `{SlotCount}`
- `{BacktrackAttempts}`, `{ConstraintViolations}`
- `{ElapsedMs}`

Use log scopes for context:
```csharp
using (_logger.BeginScope(new Dictionary<string, object> 
{ 
    ["TournamentId"] = tournamentId 
}))
{
    // All logs include TournamentId
}
```

### 6.5 Error Handling Strategy
**Location**: `src/ScheduleGenerator.Console/Program.cs` and services

Define custom exceptions:
- `InvalidTournamentConfigurationException`: validation failures
- `SchedulingInfeasibleException`: no solution possible
- `AlgorithmTimeoutException`: exceeded max time/depth

Global exception handler in Program.cs:
- Catch unhandled exceptions
- Log with ERROR level
- Display friendly message to user
- Return appropriate exit codes (0 = success, 1 = user error, 2 = system error)

Graceful degradation:
- If optimal scheduling fails, offer partial solution
- If soft constraints too restrictive, suggest disabling some

## Tests to Write

- Configuration binding tests: verify options load correctly
- DI registration tests: resolve all services without errors
- Logging tests: verify structured properties included
- Exception handling tests: verify user-friendly messages

## Success Criteria

- Configuration loads from JSON without errors
- `ValidateOnStart()` catches invalid configuration early
- All services resolve through DI container
- Logs are structured and queryable
- Exception messages guide users to resolution
- No secrets or sensitive data in configuration files
