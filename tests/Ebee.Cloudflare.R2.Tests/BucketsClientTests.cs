using Amazon.S3;
using Amazon.S3.Model;
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

    // Constructor Tests
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

    // ListBucketsAsync Tests
    [Fact]
    public async Task ListBucketsAsync_WithValidResponse_ShouldReturnMappedBuckets()
    {
        // Arrange
        var expectedBuckets = new List<S3Bucket>
        {
            new() { BucketName = "bucket1", CreationDate = DateTime.UtcNow.AddDays(-1) },
            new() { BucketName = "bucket2", CreationDate = DateTime.UtcNow.AddDays(-2) }
        };

        var s3Response = new ListBucketsResponse
        {
            Buckets = expectedBuckets,
            Owner = new Owner { DisplayName = "test-owner" }
        };

        _mockS3Client.Setup(x => x.ListBucketsAsync(
            It.IsAny<ListBucketsRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _bucketsClient.ListBucketsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Buckets.Should().HaveCount(2);
        result.Owner.Should().Be("test-owner");

        result.Buckets[0].Name.Should().Be("bucket1");
        result.Buckets[0].CreationDate.Should().Be(expectedBuckets[0].CreationDate);

        result.Buckets[1].Name.Should().Be("bucket2");
        result.Buckets[1].CreationDate.Should().Be(expectedBuckets[1].CreationDate);
    }

    [Fact]
    public async Task ListBucketsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var s3Response = new ListBucketsResponse
        {
            Buckets = [],
            Owner = null
        };

        _mockS3Client.Setup(x => x.ListBucketsAsync(
            It.IsAny<ListBucketsRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _bucketsClient.ListBucketsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Buckets.Should().BeEmpty();
        result.Owner.Should().BeNull();
    }

    [Fact]
    public async Task ListBucketsAsync_WithAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var s3Exception = new AmazonS3Exception("S3 error occurred");

        _mockS3Client.Setup(x => x.ListBucketsAsync(
            It.IsAny<ListBucketsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _bucketsClient.ListBucketsAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("Failed to list buckets: S3 error occurred");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("S3 error occurred");
    }

    [Fact]
    public async Task ListBucketsAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.ListBucketsAsync(
            It.IsAny<ListBucketsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _bucketsClient.ListBucketsAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while listing buckets: General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }
}
