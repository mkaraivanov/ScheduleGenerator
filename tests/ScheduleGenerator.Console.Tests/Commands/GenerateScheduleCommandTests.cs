using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Console.Commands;
using Xunit;

namespace ScheduleGenerator.Console.Tests.Commands;

public class GenerateScheduleCommandTests
{
    [Fact]
    public void GenerateScheduleCommand_Should_BeInstantiable()
    {
        // Arrange
        var mockOrchestrator = new Mock<ITournamentOrchestrator>();
        var mockLogger = new Mock<ILogger<GenerateScheduleCommand>>();

        // Act
        var command = new GenerateScheduleCommand(mockOrchestrator.Object, mockLogger.Object);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("generate");
    }

    [Fact]
    public void GenerateScheduleCommand_Constructor_Should_ThrowOnNullOrchestrator()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateScheduleCommand>>();

        // Act
        var act = () => new GenerateScheduleCommand(null!, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("orchestrator");
    }

    [Fact]
    public void GenerateScheduleCommand_Constructor_Should_ThrowOnNullLogger()
    {
        // Arrange
        var mockOrchestrator = new Mock<ITournamentOrchestrator>();

        // Act
        var act = () => new GenerateScheduleCommand(mockOrchestrator.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // Note: Full command execution tests would require input/output redirection
    // or integration tests with scripted console input
}
