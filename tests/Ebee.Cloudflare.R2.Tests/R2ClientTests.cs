using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.SignedUrls;
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
        var mockSignedUrlsClient = new Mock<ISignedUrlsClient>();

        // Act
        var client = new R2Client(
            mockBucketsClient.Object,
            mockObjectsClient.Object,
            mockSignedUrlsClient.Object);

        // Assert
        client.Buckets.Should().Be(mockBucketsClient.Object);
        client.Objects.Should().Be(mockObjectsClient.Object);
        client.SignedUrls.Should().Be(mockSignedUrlsClient.Object);
    }

    [Fact]
    public void Constructor_WithNullBucketsClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockObjectsClient = new Mock<IObjectsClient>();
        var mockSignedUrlsClient = new Mock<ISignedUrlsClient>();

        // Act
        var act = () => new R2Client(null!, mockObjectsClient.Object, mockSignedUrlsClient.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("bucketsClient");
    }

    [Fact]
    public void Constructor_WithNullObjectsClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockBucketsClient = new Mock<IBucketsClient>();
        var mockSignedUrlsClient = new Mock<ISignedUrlsClient>();

        // Act
        var act = () => new R2Client(mockBucketsClient.Object, null!, mockSignedUrlsClient.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("objectsClient");
    }

    [Fact]
    public void Constructor_WithNullSignedUrlsClient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockBucketsClient = new Mock<IBucketsClient>();
        var mockObjectsClient = new Mock<IObjectsClient>();

        // Act
        var act = () => new R2Client(mockBucketsClient.Object, mockObjectsClient.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("signedUrlsClient");
    }
}
