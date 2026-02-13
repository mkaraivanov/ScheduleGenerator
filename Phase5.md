# Phase 5: Testing Strategy (TDD)

**Goal**: Comprehensive test coverage written before implementation ensures correctness.

## Tasks

### 5.1 Domain Tests
**Location**: `tests/ScheduleGenerator.Domain.Tests/`

Test value objects:
- `TimeSlotTests`: validation rules, immutability, equality
- `SlotTests`: unique ID generation, equality comparison
- `MatchDurationTests`: total calculation
- `ConstraintViolationTests`: serialization

Test constraints (each independently):
- Every constraint gets dedicated test class
- Test violation detection with various scenarios
- Test penalty calculation for soft constraints
- Use `[Theory]` with `[InlineData]` for multiple cases

Test tournament generators:
- `RoundRobinGeneratorTests`: 
  - Even teams (4, 6, 8)
  - Odd teams (3, 5, 7) with BYE
  - Correct round count
  - Each team plays all others once
- `GroupStageGeneratorTests`:
  - Various group counts
  - Seeded distribution
  - Random distribution
- `KnockoutGeneratorTests`:
  - Power-of-2 teams (4, 8, 16)
  - Non-power-of-2 with byes (5, 7, 12)
  - Bracket structure verification

Test entity invariants:
- Match requires two different teams
- Tournament requires at least 2 teams
- Field availability must be future times

### 5.2 Application Tests
**Location**: `tests/ScheduleGenerator.Application.Tests/`

Test orchestration:
- `TournamentOrchestratorTests`: 
  - Mock all dependencies (Moq or NSubstitute)
  - Verify correct service call sequence
  - Error handling and propagation
- `MatchGenerationServiceTests`: each format produces expected output
- `SchedulingServiceTests`: coordinates engine and diagnostics
- `SlotGenerationServiceTests`: correct slot count, blackout handling

Test validators:
- Each validator tested independently
- Verify specific error messages for each validation rule
- Test valid inputs pass
- Test edge cases

### 5.3 Infrastructure Tests
**Location**: `tests/ScheduleGenerator.Application.Tests/` & Integration

Test backtracking algorithm:
- Small solvable tournaments (known solution)
- Unsolvable scenarios (verify correct failure)
- Constraint combinations
- Heuristic effectiveness (compare MRV vs random ordering)

Performance tests with BenchmarkDotNet:
- 8-team round-robin
- 16-team knockout
- 32-team group stage
- Measure: time, allocations, backtrack attempts

### 5.4 Integration Tests
**Location**: `tests/ScheduleGenerator.Integration.Tests/`

End-to-end scenarios:
- 6-team round-robin, 2 fields, 1 day → verify complete schedule
- 16-team knockout, 4 fields, 2 days → verify bracket progression
- 24-team group stage (4 groups), 3 fields → verify group separation
- Unsolvable: 10 teams, 1 field, insufficient time → verify diagnostic explains why
- Multiple days with blackouts → verify no matches in blackouts

Test realistic constraints:
- Travel constraints honored
- Seeded teams separated in early rounds
- Balanced kickoff times across teams

### 5.5 Architecture Tests
Optional but recommended:

Use NetArchTest.Rules:
- Verify Domain has no dependencies on other projects
- Verify Application doesn't reference Infrastructure
- Verify no circular dependencies
- Verify naming conventions

## Test Execution

Commands:
```bash
dotnet test                                    # Run all tests
dotnet test --filter "Category=Unit"          # Unit tests only
dotnet test --filter "Category=Integration"   # Integration only
dotnet test --collect:"XPlat Code Coverage"   # With coverage
```

## Success Criteria

- **Coverage goal**: >80% for Domain and Application layers
- All tests pass consistently
- Fast unit tests (< 100ms each)
- Integration tests complete in reasonable time (< 30s total)
- No flaky tests (run 10 times, all pass)
- Performance benchmarks establish baseline metrics
