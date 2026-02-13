# Phase 3: Scheduling Engine & Algorithms

**Goal**: Implement constraint-based scheduling with backtracking algorithm.

## Tasks

### 3.1 Scheduling Engine Interface
**Location**: `src/ScheduleGenerator.Application/Interfaces/`

Define:
- `ISchedulingEngine`: `Schedule(matches, slots, constraints)` → `Schedule` result

### 3.2 Backtracking Implementation
**Location**: `src/ScheduleGenerator.Infrastructure/Algorithms/`

Implement `BacktrackingScheduler`:
- `ScheduleState`: tracks assignments, available slots, team schedules
- **Variable ordering**: Most Restricted Variable (MRV) - match with fewest feasible slots
- **Value ordering**: Least Constraining Value (LCV) - slot leaving most options
- **Forward checking**: after assignment, update feasible slots for remaining matches
- **Constraint propagation**: prune domains based on hard constraints
- **Backtrack**: undo assignment when no valid slot available

Algorithm flow:
1. Select most constrained unassigned match
2. Order candidate slots by constraint impact
3. Assign match to best slot
4. Check all constraints (fail if hard violated)
5. Forward check and recurse OR backtrack

### 3.3 Constraint Evaluation
**Location**: `src/ScheduleGenerator.Infrastructure/Constraints/`

Implement:
- `ConstraintEvaluator`: aggregate violations from all constraints
- `ConstraintRegistry`: manage constraint collection, dynamic registration
- Cache expensive constraint checks
- Calculate total penalty from soft constraint violations

### 3.4 Slot Generation Implementation
**Location**: `src/ScheduleGenerator.Infrastructure/Slots/`

Implement `SlotGenerator`:
- For each field's availability windows
- Step through with (match_duration + buffer) increment
- Skip blackout times
- Generate unique slot IDs
- Validate no overlapping slots per field

## Tests to Write First (TDD)

- `BacktrackingSchedulerTests`: 
  - Solvable small tournament (3 teams, 1 field)
  - Unsolvable scenario (insufficient slots)
  - Various constraint combinations
- `ScheduleStateTests`: track assignments correctly
- `ConstraintEvaluatorTests`: aggregate violations, calculate penalties
- `SlotGeneratorTests`: correct slot count, blackout handling
- Performance tests with BenchmarkDotNet: 16-team, 32-team tournaments

## Success Criteria

- Scheduler solves feasible tournaments
- Returns diagnostic for infeasible scenarios explaining why
- MRV and LCV heuristics measurably improve performance vs naïve ordering
- All hard constraints respected in output
- Soft constraints optimized (lower penalty scores preferred)
- Performance: 32-team knockout scheduled in < 5 seconds
