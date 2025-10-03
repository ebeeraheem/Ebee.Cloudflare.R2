using Ebee.Cloudflare.R2.Buckets;
using FluentAssertions;
using Moq;

namespace Ebee.Cloudflare.R2.Tests;

/// <summary>
/// Unit tests for <see cref="R2Client"/>.
/// </summary>
public class R2ClientTests
{
    [Fact]
    public void Constructor_WithValidBucketsClient_ShouldSetBucketsProperty()
    {
        // Arrange
        var mockBucketsClient = new Mock<IBucketsClient>();

        // Act
        var client = new R2Client(mockBucketsClient.Object);

        // Assert
        client.Buckets.Should().Be(mockBucketsClient.Object);
    }

    [Fact]
    public void Constructor_WithNullBucketsClient_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new R2Client(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
