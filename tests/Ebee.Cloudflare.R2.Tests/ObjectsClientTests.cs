using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.Objects.Models;
using FluentAssertions;
using Moq;

namespace Ebee.Cloudflare.R2.Tests;

/// <summary>
/// Unit tests for <see cref="ObjectsClient"/>.
/// </summary>
public class ObjectsClientTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly ObjectsClient _objectsClient;

    public ObjectsClientTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _objectsClient = new ObjectsClient(_mockS3Client.Object);
    }

    // Constructor Tests
    [Fact]
    public void Constructor_WithValidS3Client_ShouldCreateInstance()
    {
        // Arrange
        var mockS3Client = new Mock<IAmazonS3>();

        // Act
        var client = new ObjectsClient(mockS3Client.Object);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullS3Client_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new ObjectsClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("s3Client");
    }

    // ListObjectsAsync Tests
    [Fact]
    public async Task ListObjectsAsync_WithValidResponse_ShouldReturnMappedObjects()
    {
        // Arrange
        var request = new R2ListObjectsRequest { BucketName = "test-bucket" };
        var expectedObjects = new List<S3Object>
        {
            new()
            {
                Key = "object1.txt",
                Size = 1024,
                LastModified = DateTime.UtcNow.AddDays(-1),
                ETag = "\"etag1\"",
                StorageClass = "STANDARD",
                Owner = new Owner { DisplayName = "owner1" }
            },
            new()
            {
                Key = "object2.txt",
                Size = 2048,
                LastModified = DateTime.UtcNow.AddDays(-2),
                ETag = "\"etag2\"",
                StorageClass = "STANDARD",
                Owner = new Owner { DisplayName = "owner2" }
            }
        };

        var s3Response = new ListObjectsV2Response
        {
            S3Objects = expectedObjects,
            Prefix = "prefix/",
            Delimiter = "/",
            MaxKeys = 1000,
            IsTruncated = false,
            NextContinuationToken = null,
            KeyCount = 2,
            CommonPrefixes = ["common1/", "common2/"]
        };

        _mockS3Client.Setup(x => x.ListObjectsV2Async(
            It.IsAny<ListObjectsV2Request>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.ListObjectsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Objects.Should().HaveCount(2);
        result.Prefix.Should().Be("prefix/");
        result.Delimiter.Should().Be("/");
        result.MaxKeys.Should().Be(1000);
        result.IsTruncated.Should().BeFalse();
        result.NextContinuationToken.Should().BeNull();
        result.KeyCount.Should().Be(2);
        result.CommonPrefixes.Should().HaveCount(2);

        result.Objects[0].Key.Should().Be("object1.txt");
        result.Objects[0].Size.Should().Be(1024);
        result.Objects[0].ETag.Should().Be("\"etag1\"");
        result.Objects[0].StorageClass.Should().Be("STANDARD");
        result.Objects[0].Owner.Should().Be("owner1");

        result.Objects[1].Key.Should().Be("object2.txt");
        result.Objects[1].Size.Should().Be(2048);
        result.Objects[1].ETag.Should().Be("\"etag2\"");
        result.Objects[1].StorageClass.Should().Be("STANDARD");
        result.Objects[1].Owner.Should().Be("owner2");
    }

    [Fact]
    public async Task ListObjectsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new R2ListObjectsRequest { BucketName = "test-bucket" };
        var s3Response = new ListObjectsV2Response
        {
            S3Objects = [],
            MaxKeys = 1000,
            IsTruncated = false,
            KeyCount = 0,
            CommonPrefixes = []
        };

        _mockS3Client.Setup(x => x.ListObjectsV2Async(
            It.IsAny<ListObjectsV2Request>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.ListObjectsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Objects.Should().BeEmpty();
        result.CommonPrefixes.Should().BeEmpty();
        result.KeyCount.Should().Be(0);
    }

    [Fact]
    public async Task ListObjectsAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _objectsClient.ListObjectsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task ListObjectsAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2ListObjectsRequest { BucketName = string.Empty };

        // Act
        var act = async () => await _objectsClient.ListObjectsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListObjectsAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListObjectsRequest { BucketName = "non-existent-bucket" };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.ListObjectsV2Async(
            It.IsAny<ListObjectsV2Request>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.ListObjectsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task ListObjectsAsync_WithAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListObjectsRequest { BucketName = "test-bucket" };
        var s3Exception = new AmazonS3Exception("S3 error occurred");

        _mockS3Client.Setup(x => x.ListObjectsV2Async(
            It.IsAny<ListObjectsV2Request>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.ListObjectsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("Failed to list objects in bucket 'test-bucket': S3 error occurred");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("S3 error occurred");
    }

    [Fact]
    public async Task ListObjectsAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListObjectsRequest { BucketName = "test-bucket" };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.ListObjectsV2Async(
            It.IsAny<ListObjectsV2Request>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _objectsClient.ListObjectsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while listing objects in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // GetObjectAsync Tests
    [Fact]
    public async Task GetObjectAsync_WithValidResponse_ShouldReturnObjectWithContent()
    {
        // Arrange
        var request = new R2GetObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var contentBytes = "Hello, World!"u8.ToArray();
        var responseStream = new MemoryStream(contentBytes);

        var s3Response = new GetObjectResponse
        {
            ResponseStream = responseStream,
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ETag = "\"etag123\"",
            LastModified = DateTime.UtcNow.AddDays(-1),
            VersionId = "version123"
        };

        s3Response.Headers.ContentType = "text/plain";
        s3Response.Headers.ContentLength = contentBytes.Length;
        s3Response.Headers.CacheControl = "max-age=3600";
        s3Response.Headers.ContentDisposition = "attachment; filename=test.txt";
        s3Response.Headers.ContentEncoding = "gzip";
        s3Response.ExpiresString = DateTime.UtcNow.AddHours(1).ToString("R");
        s3Response.Metadata.Add("custom-key", "custom-value");

        _mockS3Client.Setup(x => x.GetObjectAsync(
            It.IsAny<GetObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.GetObjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.ContentBytes.Should().BeEquivalentTo(contentBytes);
        result.ContentStream.Should().NotBeNull();
        result.ContentType.Should().Be("text/plain");
        result.ContentLength.Should().Be(contentBytes.Length);
        result.ETag.Should().Be("\"etag123\"");
        result.VersionId.Should().Be("version123");
        result.CacheControl.Should().Be("max-age=3600");
        result.ContentDisposition.Should().Be("attachment; filename=test.txt");
        result.ContentEncoding.Should().Be("gzip");
        result.Expires.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
        result.Metadata.Should().ContainKey("x-amz-meta-custom-key").WhoseValue.Should().Be("custom-value"); // S3 prefixes custom metadata keys with "x-amz-meta-"

        // Verify content stream contains the same data
        result.ContentStream!.Position = 0;
        var streamBytes = new byte[result.ContentStream.Length];
        await result.ContentStream.ReadAsync(streamBytes);
        streamBytes.Should().BeEquivalentTo(contentBytes);

        // Cleanup
        result.Dispose();
    }

    [Fact]
    public async Task GetObjectAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _objectsClient.GetObjectAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task GetObjectAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GetObjectRequest { BucketName = string.Empty, Key = "test-key" };

        // Act
        var act = async () => await _objectsClient.GetObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetObjectAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GetObjectRequest { BucketName = "test-bucket", Key = string.Empty };

        // Act
        var act = async () => await _objectsClient.GetObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetObjectAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GetObjectRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.txt"
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.GetObjectAsync(
            It.IsAny<GetObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.GetObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task GetObjectAsync_WithNoSuchKeyError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GetObjectRequest
        {
            BucketName = "test-bucket",
            Key = "non-existent-object.txt"
        };
        var s3Exception = new AmazonS3Exception("Object not found") { ErrorCode = "NoSuchKey" };

        _mockS3Client.Setup(x => x.GetObjectAsync(
            It.IsAny<GetObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.GetObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Object 'non-existent-object.txt' does not exist in bucket 'test-bucket'.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Object not found");
    }

    [Fact]
    public async Task GetObjectAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GetObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.GetObjectAsync(
            It.IsAny<GetObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.GetObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to get object 'test-object.txt' from bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task GetObjectAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GetObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.GetObjectAsync(
            It.IsAny<GetObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _objectsClient.GetObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while getting object 'test-object.txt' from bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // PutObjectAsync Tests
    [Fact]
    public async Task PutObjectAsync_WithContentBytes_ShouldReturnPutResponse()
    {
        // Arrange
        var contentBytes = "Hello, World!"u8.ToArray();
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ContentBytes = contentBytes,
            ContentType = "text/plain"
        };

        var s3Response = new PutObjectResponse
        {
            ETag = "\"etag123\"",
            VersionId = "version123"
        };

        _mockS3Client.Setup(x => x.PutObjectAsync(
            It.IsAny<PutObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.PutObjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.ETag.Should().Be("\"etag123\"");
        result.VersionId.Should().Be("version123");
        result.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PutObjectAsync_WithContentStream_ShouldReturnPutResponse()
    {
        // Arrange
        var contentBytes = "Hello, World!"u8.ToArray();
        var contentStream = new MemoryStream(contentBytes);
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ContentStream = contentStream,
            ContentType = "text/plain"
        };

        var s3Response = new PutObjectResponse
        {
            ETag = "\"etag123\"",
            VersionId = "version123"
        };

        _mockS3Client.Setup(x => x.PutObjectAsync(
            It.IsAny<PutObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.PutObjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.ETag.Should().Be("\"etag123\"");
        result.VersionId.Should().Be("version123");

        // Cleanup
        await contentStream.DisposeAsync();
    }

    [Fact]
    public async Task PutObjectAsync_WithFilePath_ShouldReturnPutResponse()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            FilePath = "/path/to/file.txt",
            ContentType = "text/plain"
        };

        var s3Response = new PutObjectResponse
        {
            ETag = "\"etag123\"",
            VersionId = "version123"
        };

        _mockS3Client.Setup(x => x.PutObjectAsync(
            It.IsAny<PutObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.PutObjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.ETag.Should().Be("\"etag123\"");
        result.VersionId.Should().Be("version123");
    }

    [Fact]
    public async Task PutObjectAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _objectsClient.PutObjectAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task PutObjectAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = string.Empty,
            Key = "test-key",
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PutObjectAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty,
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PutObjectAsync_WithNoContentSource_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-key"
            // No content source provided
        };

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Exactly one content source must be provided. Set either ContentStream, ContentBytes, or FilePath.");
    }

    [Fact]
    public async Task PutObjectAsync_WithMultipleContentSources_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            ContentBytes = "test"u8.ToArray(),
            ContentStream = new MemoryStream(),
            FilePath = "/path/to/file.txt"
        };

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Only one content source can be provided. Set either ContentStream, ContentBytes, or FilePath, but not multiple.");

        // Cleanup
        await request.ContentStream.DisposeAsync();
    }

    [Fact]
    public async Task PutObjectAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.txt",
            ContentBytes = "test"u8.ToArray()
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.PutObjectAsync(
            It.IsAny<PutObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task PutObjectAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ContentBytes = "test"u8.ToArray()
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.PutObjectAsync(
            It.IsAny<PutObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to put object 'test-object.txt' to bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task PutObjectAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2PutObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ContentBytes = "test"u8.ToArray()
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.PutObjectAsync(
            It.IsAny<PutObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _objectsClient.PutObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while putting object 'test-object.txt' to bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // DeleteObjectAsync Tests
    [Fact]
    public async Task DeleteObjectAsync_WithValidRequest_ShouldReturnDeleteResponse()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var s3Response = new DeleteObjectResponse
        {
            VersionId = "version123",
            DeleteMarker = "false"
        };

        _mockS3Client.Setup(x => x.DeleteObjectAsync(
            It.IsAny<DeleteObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _objectsClient.DeleteObjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.VersionId.Should().Be("version123");
        result.DeleteMarker.Should().BeFalse();
        result.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteObjectAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task DeleteObjectAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = string.Empty,
            Key = "test-key"
        };

        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteObjectAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty
        };

        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteObjectAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.txt"
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.DeleteObjectAsync(
            It.IsAny<DeleteObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task DeleteObjectAsync_WithNoSuchKeyError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = "test-bucket",
            Key = "non-existent-object.txt"
        };
        var s3Exception = new AmazonS3Exception("Object not found") { ErrorCode = "NoSuchKey" };

        _mockS3Client.Setup(x => x.DeleteObjectAsync(
            It.IsAny<DeleteObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Object 'non-existent-object.txt' does not exist in bucket 'test-bucket'.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Object not found");
    }

    [Fact]
    public async Task DeleteObjectAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.DeleteObjectAsync(
            It.IsAny<DeleteObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to delete object 'test-object.txt' from bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task DeleteObjectAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2DeleteObjectRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.DeleteObjectAsync(
            It.IsAny<DeleteObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _objectsClient.DeleteObjectAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while deleting object 'test-object.txt' from bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }
}