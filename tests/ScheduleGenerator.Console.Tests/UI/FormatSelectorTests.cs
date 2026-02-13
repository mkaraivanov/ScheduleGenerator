using FluentAssertions;
using ScheduleGenerator.Console.UI;
using Xunit;

namespace ScheduleGenerator.Console.Tests.UI;

public class FormatSelectorTests
{
    [Fact]
    public void FormatSelector_Should_BeInstantiable()
    {
        // Arrange & Act
        var selector = new FormatSelector();

        // Assert
        selector.Should().NotBeNull();
    }

    // Note: Interactive tests would require input/output redirection
    // For full testing, consider extracting logic to testable methods
    // or using integration tests with scripted input
}
