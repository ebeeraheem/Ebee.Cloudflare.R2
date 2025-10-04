using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.MultipartUploads;
using Ebee.Cloudflare.R2.MultipartUploads.Models;
using FluentAssertions;
using Moq;

namespace Ebee.Cloudflare.R2.Tests;

/// <summary>
/// Unit tests for <see cref="MultipartUploadsClient"/>.
/// </summary>
public class MultipartUploadsClientTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly MultipartUploadsClient _multipartUploadsClient;

    public MultipartUploadsClientTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _multipartUploadsClient = new MultipartUploadsClient(_mockS3Client.Object);
    }

    // Constructor Tests
    [Fact]
    public void Constructor_WithValidS3Client_ShouldCreateInstance()
    {
        // Arrange
        var mockS3Client = new Mock<IAmazonS3>();

        // Act
        var client = new MultipartUploadsClient(mockS3Client.Object);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullS3Client_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new MultipartUploadsClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("s3Client");
    }

    // InitiateMultipartUploadAsync Tests
    [Fact]
    public async Task InitiateMultipartUploadAsync_WithValidRequest_ShouldReturnInitiateResponse()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            ContentType = "application/zip"
        };

        var s3Response = new InitiateMultipartUploadResponse
        {
            UploadId = "upload-123",
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        _mockS3Client.Setup(x => x.InitiateMultipartUploadAsync(
            It.IsAny<InitiateMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.zip");
        result.UploadId.Should().Be("upload-123");
        result.ServerSideEncryption.Should().Be("AES256");
        result.InitiatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithMetadata_ShouldSetMetadata()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            Metadata = new Dictionary<string, string>
            {
                { "author", "test-user" },
                { "version", "1.0" }
            }
        };

        var s3Response = new InitiateMultipartUploadResponse
        {
            UploadId = "upload-123"
        };

        _mockS3Client.Setup(x => x.InitiateMultipartUploadAsync(
            It.Is<InitiateMultipartUploadRequest>(req =>
                req.Metadata.Count == 2 &&
                req.Metadata["author"] == "test-user" &&
                req.Metadata["version"] == "1.0"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UploadId.Should().Be("upload-123");
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _multipartUploadsClient.InitiateMultipartUploadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = string.Empty,
            Key = "test-key"
        };

        // Act
        var act = async () => await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty
        };

        // Act
        var act = async () => await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.zip"
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.InitiateMultipartUploadAsync(
            It.IsAny<InitiateMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip"
        };
        var s3Exception = new AmazonS3Exception("S3 error occurred");

        _mockS3Client.Setup(x => x.InitiateMultipartUploadAsync(
            It.IsAny<InitiateMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("Failed to initiate multipart upload for object 'test-object.zip' in bucket 'test-bucket': S3 error occurred");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("S3 error occurred");
    }

    [Fact]
    public async Task InitiateMultipartUploadAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2InitiateMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip"
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.InitiateMultipartUploadAsync(
            It.IsAny<InitiateMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _multipartUploadsClient.InitiateMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while initiating multipart upload for object 'test-object.zip' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // UploadPartAsync Tests
    [Fact]
    public async Task UploadPartAsync_WithContentBytes_ShouldReturnUploadPartResponse()
    {
        // Arrange
        var contentBytes = "Hello, World!"u8.ToArray();
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = contentBytes
        };

        var s3Response = new UploadPartResponse
        {
            ETag = "\"etag123\"",
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.zip");
        result.UploadId.Should().Be("upload-123");
        result.PartNumber.Should().Be(1);
        result.ETag.Should().Be("\"etag123\"");
        result.ServerSideEncryption.Should().Be("AES256");
        result.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UploadPartAsync_WithContentStream_ShouldReturnUploadPartResponse()
    {
        // Arrange
        var contentBytes = "Hello, World!"u8.ToArray();
        var contentStream = new MemoryStream(contentBytes);
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentStream = contentStream
        };

        var s3Response = new UploadPartResponse
        {
            ETag = "\"etag123\""
        };

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ETag.Should().Be("\"etag123\"");
        result.PartNumber.Should().Be(1);

        // Cleanup
        await contentStream.DisposeAsync();
    }

    [Fact]
    public async Task UploadPartAsync_WithFilePath_ShouldReturnUploadPartResponse()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            PartNumber = 1,
            FilePath = "/path/to/file.part1"
        };

        var s3Response = new UploadPartResponse
        {
            ETag = "\"etag123\""
        };

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ETag.Should().Be("\"etag123\"");
        result.PartNumber.Should().Be(1);
    }

    [Fact]
    public async Task UploadPartAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task UploadPartAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = string.Empty,
            Key = "test-key",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadPartAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty,
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadPartAsync_WithEmptyUploadId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = string.Empty,
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadPartAsync_WithInvalidPartNumber_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = "upload-123",
            PartNumber = 0, // Invalid part number
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("Part number must be between 1 and 10000. (Parameter 'request')\nActual value was 0.");
    }

    [Fact]
    public async Task UploadPartAsync_WithPartNumberTooHigh_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = "upload-123",
            PartNumber = 10001, // Invalid part number
            ContentBytes = "test"u8.ToArray()
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("Part number must be between 1 and 10000. (Parameter 'request')\nActual value was 10001.");
    }

    [Fact]
    public async Task UploadPartAsync_WithNoContentSource_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = "upload-123",
            PartNumber = 1
            // No content source provided
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Exactly one content source must be provided. Set either ContentStream, ContentBytes, or FilePath.");
    }

    [Fact]
    public async Task UploadPartAsync_WithMultipleContentSources_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray(),
            ContentStream = new MemoryStream(),
            FilePath = "/path/to/file.txt"
        };

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Only one content source can be provided. Set either ContentStream, ContentBytes, or FilePath, but not multiple.");

        // Cleanup
        await request.ContentStream.DisposeAsync();
    }

    [Fact]
    public async Task UploadPartAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task UploadPartAsync_WithNoSuchUploadError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "non-existent-upload",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };
        var s3Exception = new AmazonS3Exception("Upload not found") { ErrorCode = "NoSuchUpload" };

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Multipart upload 'non-existent-upload' does not exist for object 'test-object.zip' in bucket 'test-bucket'.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Upload not found");
    }

    [Fact]
    public async Task UploadPartAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to upload part 1 for object 'test-object.zip' in bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task UploadPartAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2UploadPartRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.UploadPartAsync(
            It.IsAny<UploadPartRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _multipartUploadsClient.UploadPartAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while uploading part 1 for object 'test-object.zip' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // CompleteMultipartUploadAsync Tests
    [Fact]
    public async Task CompleteMultipartUploadAsync_WithValidRequest_ShouldReturnCompleteResponse()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            Parts =
            [
                new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" },
                new R2CompletedPart { PartNumber = 2, ETag = "\"etag2\"" }
            ]
        };

        var s3Response = new CompleteMultipartUploadResponse
        {
            ETag = "\"final-etag\"",
            Location = "https://bucket.amazonaws.com/key",
            VersionId = "version123",
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        _mockS3Client.Setup(x => x.CompleteMultipartUploadAsync(
            It.IsAny<CompleteMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.zip");
        result.ETag.Should().Be("\"final-etag\"");
        result.Location.Should().Be("https://bucket.amazonaws.com/key");
        result.VersionId.Should().Be("version123");
        result.ServerSideEncryption.Should().Be("AES256");
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = string.Empty,
            Key = "test-key",
            UploadId = "upload-123",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty,
            UploadId = "upload-123",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithEmptyUploadId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = string.Empty,
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithNoParts_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = "upload-123",
            Parts = [] // No parts provided
        };

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("At least one part must be provided to complete the multipart upload.");
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.CompleteMultipartUploadAsync(
            It.IsAny<CompleteMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithNoSuchUploadError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "non-existent-upload",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };
        var s3Exception = new AmazonS3Exception("Upload not found") { ErrorCode = "NoSuchUpload" };

        _mockS3Client.Setup(x => x.CompleteMultipartUploadAsync(
            It.IsAny<CompleteMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Multipart upload 'non-existent-upload' does not exist for object 'test-object.zip' in bucket 'test-bucket'.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Upload not found");
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithInvalidPartError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"invalid-etag\"" }]
        };
        var s3Exception = new AmazonS3Exception("Invalid part") { ErrorCode = "InvalidPart" };

        _mockS3Client.Setup(x => x.CompleteMultipartUploadAsync(
            It.IsAny<CompleteMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("One or more parts are invalid for multipart upload 'upload-123' of object 'test-object.zip' in bucket 'test-bucket': Invalid part");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Invalid part");
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.CompleteMultipartUploadAsync(
            It.IsAny<CompleteMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to complete multipart upload for object 'test-object.zip' in bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task CompleteMultipartUploadAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2CompleteMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123",
            Parts = [new R2CompletedPart { PartNumber = 1, ETag = "\"etag1\"" }]
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.CompleteMultipartUploadAsync(
            It.IsAny<CompleteMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _multipartUploadsClient.CompleteMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while completing multipart upload for object 'test-object.zip' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // AbortMultipartUploadAsync Tests
    [Fact]
    public async Task AbortMultipartUploadAsync_WithValidRequest_ShouldReturnAbortResponse()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };

        var s3Response = new AbortMultipartUploadResponse();

        _mockS3Client.Setup(x => x.AbortMultipartUploadAsync(
            It.IsAny<AbortMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.zip");
        result.UploadId.Should().Be("upload-123");
        result.AbortedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = string.Empty,
            Key = "test-key",
            UploadId = "upload-123"
        };

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty,
            UploadId = "upload-123"
        };

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithEmptyUploadId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = string.Empty
        };

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.AbortMultipartUploadAsync(
            It.IsAny<AbortMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithNoSuchUploadError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "non-existent-upload"
        };
        var s3Exception = new AmazonS3Exception("Upload not found") { ErrorCode = "NoSuchUpload" };

        _mockS3Client.Setup(x => x.AbortMultipartUploadAsync(
            It.IsAny<AbortMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Multipart upload 'non-existent-upload' does not exist for object 'test-object.zip' in bucket 'test-bucket'.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Upload not found");
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.AbortMultipartUploadAsync(
            It.IsAny<AbortMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to abort multipart upload 'upload-123' for object 'test-object.zip' in bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task AbortMultipartUploadAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.AbortMultipartUploadAsync(
            It.IsAny<AbortMultipartUploadRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _multipartUploadsClient.AbortMultipartUploadAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while aborting multipart upload 'upload-123' for object 'test-object.zip' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // ListPartsAsync Tests
    [Fact]
    public async Task ListPartsAsync_WithValidResponse_ShouldReturnMappedParts()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };

        var expectedParts = new List<PartDetail>
        {
            new()
            {
                PartNumber = 1,
                ETag = "\"etag1\"",
                Size = 1024,
                LastModified = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                PartNumber = 2,
                ETag = "\"etag2\"",
                Size = 2048,
                LastModified = DateTime.UtcNow.AddMinutes(-3)
            }
        };

        var s3Response = new ListPartsResponse
        {
            Parts = expectedParts,
            MaxParts = 1000,
            IsTruncated = false,
            NextPartNumberMarker = null,
            StorageClass = "STANDARD",
            Owner = new Owner { DisplayName = "test-owner" }
        };

        _mockS3Client.Setup(x => x.ListPartsAsync(
            It.IsAny<ListPartsRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.zip");
        result.UploadId.Should().Be("upload-123");
        result.Parts.Should().HaveCount(2);
        result.MaxParts.Should().Be(1000);
        result.IsTruncated.Should().BeFalse();
        result.NextPartNumberMarker.Should().Be(null);
        result.StorageClass.Should().Be("STANDARD");
        result.Owner.Should().Be("test-owner");

        result.Parts[0].PartNumber.Should().Be(1);
        result.Parts[0].ETag.Should().Be("\"etag1\"");
        result.Parts[0].Size.Should().Be(1024);

        result.Parts[1].PartNumber.Should().Be(2);
        result.Parts[1].ETag.Should().Be("\"etag2\"");
        result.Parts[1].Size.Should().Be(2048);
    }

    [Fact]
    public async Task ListPartsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };

        var s3Response = new ListPartsResponse
        {
            Parts = [],
            MaxParts = 1000,
            IsTruncated = false
        };

        _mockS3Client.Setup(x => x.ListPartsAsync(
            It.IsAny<ListPartsRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Parts.Should().BeEmpty();
    }

    [Fact]
    public async Task ListPartsAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task ListPartsAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = string.Empty,
            Key = "test-key",
            UploadId = "upload-123"
        };

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListPartsAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty,
            UploadId = "upload-123"
        };

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListPartsAsync_WithEmptyUploadId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = "test-key",
            UploadId = string.Empty
        };

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListPartsAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "non-existent-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.ListPartsAsync(
            It.IsAny<ListPartsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task ListPartsAsync_WithNoSuchUploadError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "non-existent-upload"
        };
        var s3Exception = new AmazonS3Exception("Upload not found") { ErrorCode = "NoSuchUpload" };

        _mockS3Client.Setup(x => x.ListPartsAsync(
            It.IsAny<ListPartsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Multipart upload 'non-existent-upload' does not exist for object 'test-object.zip' in bucket 'test-bucket'.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Upload not found");
    }

    [Fact]
    public async Task ListPartsAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.ListPartsAsync(
            It.IsAny<ListPartsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to list parts for multipart upload 'upload-123' of object 'test-object.zip' in bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task ListPartsAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListPartsRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.zip",
            UploadId = "upload-123"
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.ListPartsAsync(
            It.IsAny<ListPartsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _multipartUploadsClient.ListPartsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while listing parts for multipart upload 'upload-123' of object 'test-object.zip' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // ListMultipartUploadsAsync Tests
    [Fact]
    public async Task ListMultipartUploadsAsync_WithValidResponse_ShouldReturnMappedUploads()
    {
        // Arrange
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = "test-bucket"
        };

        var expectedUploads = new List<MultipartUpload>
        {
            new()
            {
                Key = "object1.zip",
                UploadId = "upload1",
                Initiated = DateTime.UtcNow.AddHours(-1),
                StorageClass = "STANDARD",
                Owner = new Owner { DisplayName = "owner1" }
            },
            new()
            {
                Key = "object2.zip",
                UploadId = "upload2",
                Initiated = DateTime.UtcNow.AddHours(-2),
                StorageClass = "STANDARD",
                Owner = new Owner { DisplayName = "owner2" }
            }
        };

        var s3Response = new ListMultipartUploadsResponse
        {
            MultipartUploads = expectedUploads,
            Prefix = "prefix/",
            Delimiter = "/",
            MaxUploads = 1000,
            IsTruncated = false,
            NextKeyMarker = null,
            NextUploadIdMarker = null,
            CommonPrefixes = ["common1/", "common2/"]
        };

        _mockS3Client.Setup(x => x.ListMultipartUploadsAsync(
            It.IsAny<ListMultipartUploadsRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.ListMultipartUploadsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Uploads.Should().HaveCount(2);
        result.Prefix.Should().Be("prefix/");
        result.Delimiter.Should().Be("/");
        result.MaxUploads.Should().Be(1000);
        result.IsTruncated.Should().BeFalse();
        result.NextKeyMarker.Should().BeNull();
        result.NextUploadIdMarker.Should().BeNull();
        result.CommonPrefixes.Should().HaveCount(2);

        result.Uploads[0].Key.Should().Be("object1.zip");
        result.Uploads[0].UploadId.Should().Be("upload1");
        result.Uploads[0].StorageClass.Should().Be("STANDARD");
        result.Uploads[0].Owner.Should().Be("owner1");

        result.Uploads[1].Key.Should().Be("object2.zip");
        result.Uploads[1].UploadId.Should().Be("upload2");
        result.Uploads[1].StorageClass.Should().Be("STANDARD");
        result.Uploads[1].Owner.Should().Be("owner2");
    }

    [Fact]
    public async Task ListMultipartUploadsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = "test-bucket"
        };

        var s3Response = new ListMultipartUploadsResponse
        {
            MultipartUploads = [],
            MaxUploads = 1000,
            IsTruncated = false,
            CommonPrefixes = []
        };

        _mockS3Client.Setup(x => x.ListMultipartUploadsAsync(
            It.IsAny<ListMultipartUploadsRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(s3Response);

        // Act
        var result = await _multipartUploadsClient.ListMultipartUploadsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Uploads.Should().BeEmpty();
        result.CommonPrefixes.Should().BeEmpty();
    }

    [Fact]
    public async Task ListMultipartUploadsAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _multipartUploadsClient.ListMultipartUploadsAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
                 .WithParameterName("request");
    }

    [Fact]
    public async Task ListMultipartUploadsAsync_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = string.Empty
        };

        // Act
        var act = async () => await _multipartUploadsClient.ListMultipartUploadsAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListMultipartUploadsAsync_WithNoSuchBucketError_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = "non-existent-bucket"
        };
        var s3Exception = new AmazonS3Exception("Bucket not found") { ErrorCode = "NoSuchBucket" };

        _mockS3Client.Setup(x => x.ListMultipartUploadsAsync(
            It.IsAny<ListMultipartUploadsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.ListMultipartUploadsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Bucket 'non-existent-bucket' does not exist.");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Bucket not found");
    }

    [Fact]
    public async Task ListMultipartUploadsAsync_WithOtherAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = "test-bucket"
        };
        var s3Exception = new AmazonS3Exception("Other S3 error") { ErrorCode = "OtherError" };

        _mockS3Client.Setup(x => x.ListMultipartUploadsAsync(
            It.IsAny<ListMultipartUploadsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(s3Exception);

        // Act
        var act = async () => await _multipartUploadsClient.ListMultipartUploadsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
                 .WithMessage("Failed to list multipart uploads in bucket 'test-bucket': Other S3 error");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("Other S3 error");
    }

    [Fact]
    public async Task ListMultipartUploadsAsync_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = "test-bucket"
        };
        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.ListMultipartUploadsAsync(
            It.IsAny<ListMultipartUploadsRequest>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(generalException);

        // Act
        var act = async () => await _multipartUploadsClient.ListMultipartUploadsAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<R2Exception>()
            .WithMessage("An unexpected error occurred while listing multipart uploads in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }
}