using Amazon.S3;
using Ebee.Cloudflare.R2.Buckets;
using FluentAssertions;
using Moq;

namespace Ebee.Cloudflare.R2.Tests;

/// <summary>
/// Unit tests for <see cref="BucketsClient"/>.
/// </summary>
public class BucketsClientTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly BucketsClient _bucketsClient;

    public BucketsClientTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _bucketsClient = new BucketsClient(_mockS3Client.Object);
    }

    [Fact]
    public void Constructor_WithValidS3Client_ShouldCreateInstance()
    {
        // Arrange
        var mockS3Client = new Mock<IAmazonS3>();

        // Act
        var client = new BucketsClient(mockS3Client.Object);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullS3Client_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new BucketsClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("s3Client");
    }


}
