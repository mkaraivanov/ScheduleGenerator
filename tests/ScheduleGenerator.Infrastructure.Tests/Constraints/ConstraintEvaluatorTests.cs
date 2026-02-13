using FluentAssertions;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;
using ScheduleGenerator.Infrastructure.Constraints;
using Xunit;

namespace ScheduleGenerator.Infrastructure.Tests.Constraints;

public class ConstraintEvaluatorTests
{
    [Fact]
    public void Evaluate_WithNoViolations_ReturnsEmptyList()
    {
        // Arrange
        var registry = new ConstraintRegistry();
        var evaluator = new ConstraintEvaluator(registry);
        var schedule = new Schedule(Guid.NewGuid());

        // Act
        var violations = evaluator.Evaluate(schedule);

        // Assert
        violations.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WithViolations_ReturnsViolationList()
    {
        // Arrange
        var registry = new ConstraintRegistry();
        var mockConstraint = new MockViolatingConstraint();
        registry.Register(mockConstraint);
        
        var evaluator = new ConstraintEvaluator(registry);
        var schedule = new Schedule(Guid.NewGuid());

        // Act
        var violations = evaluator.Evaluate(schedule);

        // Assert
        violations.Should().HaveCount(1);
        violations.First().ConstraintName.Should().Be("Mock Violation");
    }

    [Fact]
    public void CalculateTotalPenalty_SumsPenalties()
    {
        // Arrange
        var registry = new ConstraintRegistry();
        registry.Register(new MockSoftConstraint(penalty: 10));
        registry.Register(new MockSoftConstraint(penalty: 20));
        
        var evaluator = new ConstraintEvaluator(registry);
        var schedule = new Schedule(Guid.NewGuid());

        // Act
        var totalPenalty = evaluator.CalculateTotalPenalty(schedule);

        // Assert
        totalPenalty.Should().Be(30);
    }

    [Fact]
    public void SatisfiesHardConstraints_WithNoViolations_ReturnsTrue()
    {
        // Arrange
        var registry = new ConstraintRegistry();
        registry.Register(new MockSatisfiedConstraint(isHard: true));
        
        var evaluator = new ConstraintEvaluator(registry);
        var schedule = new Schedule(Guid.NewGuid());

        // Act
        var satisfies = evaluator.SatisfiesHardConstraints(schedule);

        // Assert
        satisfies.Should().BeTrue();
    }

    [Fact]
    public void SatisfiesHardConstraints_WithViolations_ReturnsFalse()
    {
        // Arrange
        var registry = new ConstraintRegistry();
        registry.Register(new MockViolatingConstraint(isHard: true));
        
        var evaluator = new ConstraintEvaluator(registry);
        var schedule = new Schedule(Guid.NewGuid());

        // Act
        var satisfies = evaluator.SatisfiesHardConstraints(schedule);

        // Assert
        satisfies.Should().BeFalse();
    }

    [Fact]
    public void ViolatesHardConstraints_ChecksOnlyHardConstraints()
    {
        // Arrange
        var registry = new ConstraintRegistry();
        registry.Register(new MockViolatingConstraint(isHard: true));
        registry.Register(new MockViolatingConstraint(isHard: false)); // Soft constraint
        
        var evaluator = new ConstraintEvaluator(registry);
        var schedule = new Schedule(Guid.NewGuid());
        var match = new Match(Guid.NewGuid(), Guid.NewGuid(), MatchStage.RoundRobin, 1);

        // Act
        var violates = evaluator.ViolatesHardConstraints(schedule, match);

        // Assert
        violates.Should().BeTrue();
    }

    // Mock constraint classes for testing
    private class MockViolatingConstraint : IConstraint
    {
        public string Name => "Mock Violation";
        public ConstraintType Type { get; }
        public double Weight => 1.0;

        public MockViolatingConstraint(bool isHard = true)
        {
            Type = isHard ? ConstraintType.Hard : ConstraintType.Soft;
        }

        public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
        {
            yield return new ConstraintViolation(
                Name,
                "Test violation",
                Type == ConstraintType.Hard ? ConstraintSeverity.Critical : ConstraintSeverity.Warning,
                Type == ConstraintType.Hard,
                10);
        }
    }

    private class MockSatisfiedConstraint : IConstraint
    {
        public string Name => "Mock Satisfied";
        public ConstraintType Type { get; }
        public double Weight => 1.0;

        public MockSatisfiedConstraint(bool isHard = true)
        {
            Type = isHard ? ConstraintType.Hard : ConstraintType.Soft;
        }

        public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
        {
            return Enumerable.Empty<ConstraintViolation>();
        }
    }

    private class MockSoftConstraint : IConstraint
    {
        private readonly double _penalty;
        public string Name => "Mock Soft";
        public ConstraintType Type => ConstraintType.Soft;
        public double Weight => 1.0;

        public MockSoftConstraint(double penalty)
        {
            _penalty = penalty;
        }

        public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
        {
            yield return new ConstraintViolation(
                Name,
                "Test soft constraint",
                ConstraintSeverity.Warning,
                false,
                _penalty);
        }
    }
}
