# Phase 1: Project Structure & Domain Foundation

**Goal**: Establish solution structure and core domain model with zero external dependencies.

## Tasks

### 1.1 Solution & Projects
- Create `ScheduleGenerator.sln`
- Create four main projects (Domain, Application, Infrastructure, Console)
- Create three test projects
- Configure: .NET 9, nullable reference types, file-scoped namespaces, implicit usings
- Set up project references (dependency flow inward)

### 1.2 Domain Entities
**Location**: `src/ScheduleGenerator.Domain/Entities/`

Implement:
- `Team`: name, club (optional), seed (optional), travel constraints
- `Field`: id, availability windows (collection of TimeSlot)
- `Match`: teamA, teamB, stage enum, round number, group identifier, prerequisite matches for knockout
- `Tournament`: teams, fields, format, rules, time windows
- `Schedule`: scheduled matches, diagnostics, is feasible flag

### 1.3 Value Objects
**Location**: `src/ScheduleGenerator.Domain/ValueObjects/`

Implement (as records for immutability):
- `TimeSlot`: start/end DateTime, validation
- `Slot`: unique id, field id, time slot
- `MatchDuration`: match minutes, buffer minutes, calculated total
- `TimeWindow`: day start/end, blackout periods
- `ConstraintViolation`: name, description, severity, affected entities

### 1.4 Constraint System
**Location**: `src/ScheduleGenerator.Domain/Constraints/`

Define:
- `IConstraint` interface: `Evaluate()`, `Type`, `Weight`
- `ConstraintType` enum: Hard, Soft

Implement hard constraints:
- `NoSimultaneousMatchesConstraint`: team can't play twice concurrently
- `OneMatchPerSlotConstraint`: one match per field/time
- `MinimumRestTimeConstraint`: configurable rest between team's matches
- `StageDependencyConstraint`: knockout matches after prerequisites

Implement soft constraints:
- `BalancedKickoffTimesConstraint`: distribute early/late times fairly
- `MinimizeFieldChangesConstraint`: reduce team travel
- `OpponentSpacingConstraint`: avoid same opponents too close
- `SeededTeamSeparationConstraint`: keep top seeds apart early

### 1.5 Tournament Format System
**Location**: `src/ScheduleGenerator.Domain/Formats/`

Define:
- `ITournamentFormatGenerator` interface: `GenerateMatches()`
- `TournamentFormat`: type, legs, group count, advancement rules
- Abstract `TournamentFormatGenerator` base class (Template Method pattern)

Implement:
- `RoundRobinGenerator`: circle method, handle odd teams with BYE
- `GroupStageGenerator`: distribute teams, round-robin per group
- `KnockoutGenerator`: bracket size (power of 2), insert byes, placeholder matches

## Tests to Write First (TDD)

- `TimeSlotTests`: validation, duration calculation
- `SlotTests`: equality, uniqueness
- `NoSimultaneousMatchesConstraintTests`: detect conflicts
- `MinimumRestTimeConstraintTests`: various rest periods
- `RoundRobinGeneratorTests`: even/odd teams, correct pairings
- `GroupStageGeneratorTests`: team distribution
- `KnockoutGeneratorTests`: bracket generation, bye placement

## Success Criteria

- All domain tests pass
- Domain project has zero external dependencies (verify manually)
- Value objects are immutable
- Constraints independently testable
- Format generators produce correct match lists
