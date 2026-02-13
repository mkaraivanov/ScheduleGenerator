# Phase 2: Application Layer - Use Cases

**Goal**: Orchestrate domain logic through services with clean DTOs and validation.

## Tasks

### 2.1 Application Services
**Location**: `src/ScheduleGenerator.Application/Services/`

Define interfaces:
- `IMatchGenerationService`: coordinates format generators
- `ISchedulingService`: coordinates algorithm, constraints, slot generation
- `ISlotGenerationService`: generates all potential slots

Implement:
- `MatchGenerationService`: select format generator, execute, return matches
- `SchedulingService`: validate inputs, generate slots, invoke engine, aggregate diagnostics
- `SlotGenerationService`: create slots from fields and time windows
- `TournamentOrchestrator`: main entry point, full pipeline

### 2.2 DTOs & Models
**Location**: `src/ScheduleGenerator.Application/Models/`

Define:
- `TournamentDefinition`: mirrors JSON schema (teams array, fields, format config, rules)
- `ScheduleOutput`: scheduled matches with time/field, diagnostics section
- `ConstraintConfiguration`: weights and enabled flags
- `SchedulingRules`: match/buffer minutes, rest time, max matches per day

Map between DTOs and domain entities.

### 2.3 Input Validation
**Location**: `src/ScheduleGenerator.Application/Validators/`

Use FluentValidation:
- `TournamentDefinitionValidator`: teams not empty, valid fields, sensible windows
- `FieldValidator`: availability doesn't overlap, start before end
- `TimeWindowValidator`: sufficient duration for at least one match
- `FormatValidator`: group count compatible with teams, valid advancement rules

Each validator produces actionable error messages.

## Tests to Write First (TDD)

- `TournamentOrchestratorTests`: mock dependencies, verify orchestration flow
- `MatchGenerationServiceTests`: different formats produce correct output
- `SlotGenerationServiceTests`: various time windows, blackouts handled
- `TournamentDefinitionValidatorTests`: catch all invalid inputs
- `FieldValidatorTests`: overlapping windows rejected
- DTOs serialize/deserialize correctly

## Success Criteria

- All application tests pass
- Clear separation: DTOs in Application, entities in Domain
- Validation catches all invalid scenarios with helpful messages
- Services coordinate without business logic (logic stays in Domain)
- Orchestrator successfully chains: definition → matches → schedule
