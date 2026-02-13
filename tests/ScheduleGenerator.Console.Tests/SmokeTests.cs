using FluentAssertions;
using ScheduleGenerator.Console.Commands;
using ScheduleGenerator.Console.UI;
using Xunit;

namespace ScheduleGenerator.Console.Tests;

/// <summary>
/// Smoke tests to verify basic application functionality
/// </summary>
public class SmokeTests
{
    [Fact]
    public void ConsoleApplication_Should_HaveValidAssembly()
    {
        // Arrange
        var assembly = typeof(GenerateScheduleCommand).Assembly;

        // Assert
        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("ScheduleGenerator.Console");
    }

    [Fact]
    public void UICollectors_Should_BeAccessible()
    {
        // Arrange & Act
        var teamCollector = new TeamCollector();
        var fieldCollector = new FieldCollector();
        var timeWindowCollector = new TimeWindowCollector();
        var formatSelector = new FormatSelector();
        var rulesConfigurator = new RulesConfigurator();
        var constraintConfigurator = new ConstraintConfigurator();
        var outputRenderer = new OutputRenderer();

        // Assert
        teamCollector.Should().NotBeNull();
        fieldCollector.Should().NotBeNull();
        timeWindowCollector.Should().NotBeNull();
        formatSelector.Should().NotBeNull();
        rulesConfigurator.Should().NotBeNull();
        constraintConfigurator.Should().NotBeNull();
        outputRenderer.Should().NotBeNull();
    }
}
