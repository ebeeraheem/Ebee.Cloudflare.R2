using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Objects;
using FluentAssertions;
using Moq;

namespace Ebee.Cloudflare.R2.Tests;

/// <summary>
/// Unit tests for <see cref="R2Client"/>.
/// </summary>
public class R2ClientTests
{
    [Fact]
    public void Constructor_WithValidClients_ShouldSetProperties()
    {
        // Arrange
        var mockBucketsClient = new Mock<IBucketsClient>();
        var mockObjectsClient = new Mock<IObjectsClient>();

        // Act
        var client = new R2Client(mockBucketsClient.Object, mockObjectsClient.Object);

        // Assert
        client.Buckets.Should().Be(mockBucketsClient.Object);
        client.Objects.Should().Be(mockObjectsClient.Object);
    }

    [Fact]
    public void Constructor_WithNullBucketsClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockObjectsClient = new Mock<IObjectsClient>();

        // Act
        var act = () => new R2Client(null!, mockObjectsClient.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("bucketsClient");
    }

    [Fact]
    public void Constructor_WithNullObjectsClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockBucketsClient = new Mock<IBucketsClient>();

        // Act
        var act = () => new R2Client(mockBucketsClient.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("objectsClient");
    }
}
