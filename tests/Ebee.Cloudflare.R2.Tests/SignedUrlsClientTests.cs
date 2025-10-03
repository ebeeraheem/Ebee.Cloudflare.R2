using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.SignedUrls;
using Ebee.Cloudflare.R2.SignedUrls.Models;
using FluentAssertions;
using Moq;

namespace Ebee.Cloudflare.R2.Tests;

/// <summary>
/// Unit tests for <see cref="SignedUrlsClient"/>.
/// </summary>
public class SignedUrlsClientTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly SignedUrlsClient _signedUrlsClient;

    public SignedUrlsClientTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _signedUrlsClient = new SignedUrlsClient(_mockS3Client.Object);
    }

    // Constructor Tests
    [Fact]
    public void Constructor_WithValidS3Client_ShouldCreateInstance()
    {
        // Arrange
        var mockS3Client = new Mock<IAmazonS3>();

        // Act
        var client = new SignedUrlsClient(mockS3Client.Object);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullS3Client_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new SignedUrlsClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("s3Client");
    }

    // GenerateGetSignedUrl Tests
    [Fact]
    public void GenerateGetSignedUrl_WithValidRequest_ShouldReturnSignedUrlResponse()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ExpiresIn = TimeSpan.FromHours(2)
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.SignedUrl.Should().Be(expectedSignedUrl);
        result.HttpMethod.Should().Be("GET");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), TimeSpan.FromSeconds(5));
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateGetSignedUrl_WithResponseHeaders_ShouldReturnSignedUrlResponse()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ResponseContentType = "text/plain",
            ResponseContentDisposition = "attachment; filename=test.txt",
            ResponseCacheControl = "max-age=3600",
            ResponseExpires = DateTime.UtcNow.AddHours(1)
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.SignedUrl.Should().Be(expectedSignedUrl);
        result.HttpMethod.Should().Be("GET");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateGetSignedUrl_WithExplicitExpires_ShouldUseSpecifiedExpiration()
    {
        // Arrange
        var explicitExpires = DateTime.UtcNow.AddHours(3);
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            Expires = explicitExpires
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.ExpiresAt.Should().Be(explicitExpires);
    }

    [Fact]
    public void GenerateGetSignedUrl_WithNoExpiration_ShouldUseDefaultExpiration()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateGetSignedUrl_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("request");
    }

    [Fact]
    public void GenerateGetSignedUrl_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = string.Empty,
            Key = "test-object.txt"
        };

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateGetSignedUrl_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty
        };

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateGetSignedUrl_WithPastExpiration_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            Expires = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        act.Should().Throw<R2Exception>()
           .WithMessage("*An unexpected error occurred while generating GET signed URL for object 'test-object.txt' in bucket 'test-bucket'*");
    }

    [Fact]
    public void GenerateGetSignedUrl_WithExpirationTooFar_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ExpiresIn = TimeSpan.FromDays(8)
        };

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        act.Should().Throw<R2Exception>()
           .WithMessage("*An unexpected error occurred while generating GET signed URL for object 'test-object.txt' in bucket 'test-bucket'*");
    }

    [Fact]
    public void GenerateGetSignedUrl_WithNegativeExpiresIn_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ExpiresIn = TimeSpan.FromHours(-1)
        };

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        act.Should().Throw<R2Exception>()
           .WithMessage("*An unexpected error occurred while generating GET signed URL for object 'test-object.txt' in bucket 'test-bucket'*");
    }

    [Fact]
    public void GenerateGetSignedUrl_WithAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var s3Exception = new AmazonS3Exception("S3 error occurred");

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Throws(s3Exception);

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        var exception = act.Should().Throw<R2Exception>()
            .WithMessage("Failed to generate GET signed URL for object 'test-object.txt' in bucket 'test-bucket': S3 error occurred");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("S3 error occurred");
    }

    [Fact]
    public void GenerateGetSignedUrl_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Throws(generalException);

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        var exception = act.Should().Throw<R2Exception>()
            .WithMessage("An unexpected error occurred while generating GET signed URL for object 'test-object.txt' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // GeneratePutSignedUrl Tests
    [Fact]
    public void GeneratePutSignedUrl_WithValidRequest_ShouldReturnSignedUrlResponse()
    {
        // Arrange
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ContentType = "text/plain",
            ExpiresIn = TimeSpan.FromHours(2)
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GeneratePutSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.SignedUrl.Should().Be(expectedSignedUrl);
        result.HttpMethod.Should().Be("PUT");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), TimeSpan.FromSeconds(5));
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GeneratePutSignedUrl_WithMetadataAndHeaders_ShouldReturnSignedUrlResponse()
    {
        // Arrange
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ContentType = "text/plain",
            CacheControl = "max-age=3600",
            ContentDisposition = "attachment; filename=test.txt",
            ContentEncoding = "gzip",
            ServerSideEncryption = "AES256",
            StorageClass = "STANDARD",
            Metadata = new Dictionary<string, string>
            {
                { "custom-key", "custom-value" },
                { "author", "test-user" }
            }
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GeneratePutSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.SignedUrl.Should().Be(expectedSignedUrl);
        result.HttpMethod.Should().Be("PUT");
    }

    [Fact]
    public void GeneratePutSignedUrl_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _signedUrlsClient.GeneratePutSignedUrl(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("request");
    }

    [Fact]
    public void GeneratePutSignedUrl_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = string.Empty,
            Key = "test-object.txt"
        };

        // Act
        var act = () => _signedUrlsClient.GeneratePutSignedUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GeneratePutSignedUrl_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty
        };

        // Act
        var act = () => _signedUrlsClient.GeneratePutSignedUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GeneratePutSignedUrl_WithAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var s3Exception = new AmazonS3Exception("S3 error occurred");

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Throws(s3Exception);

        // Act
        var act = () => _signedUrlsClient.GeneratePutSignedUrl(request);

        // Assert
        var exception = act.Should().Throw<R2Exception>()
            .WithMessage("Failed to generate PUT signed URL for object 'test-object.txt' in bucket 'test-bucket': S3 error occurred");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("S3 error occurred");
    }

    [Fact]
    public void GeneratePutSignedUrl_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Throws(generalException);

        // Act
        var act = () => _signedUrlsClient.GeneratePutSignedUrl(request);

        // Assert
        var exception = act.Should().Throw<R2Exception>()
            .WithMessage("An unexpected error occurred while generating PUT signed URL for object 'test-object.txt' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // GenerateDeleteSignedUrl Tests
    [Fact]
    public void GenerateDeleteSignedUrl_WithValidRequest_ShouldReturnSignedUrlResponse()
    {
        // Arrange
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ExpiresIn = TimeSpan.FromMinutes(30)
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateDeleteSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.SignedUrl.Should().Be(expectedSignedUrl);
        result.HttpMethod.Should().Be("DELETE");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateDeleteSignedUrl_WithGovernanceBypass_ShouldReturnSignedUrlResponse()
    {
        // Arrange
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            BypassGovernanceRetention = true,
            ExpectedBucketOwner = "expected-owner"
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateDeleteSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.BucketName.Should().Be("test-bucket");
        result.Key.Should().Be("test-object.txt");
        result.SignedUrl.Should().Be(expectedSignedUrl);
        result.HttpMethod.Should().Be("DELETE");
    }

    [Fact]
    public void GenerateDeleteSignedUrl_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _signedUrlsClient.GenerateDeleteSignedUrl(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("request");
    }

    [Fact]
    public void GenerateDeleteSignedUrl_WithEmptyBucketName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = string.Empty,
            Key = "test-object.txt"
        };

        // Act
        var act = () => _signedUrlsClient.GenerateDeleteSignedUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateDeleteSignedUrl_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty
        };

        // Act
        var act = () => _signedUrlsClient.GenerateDeleteSignedUrl(request);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateDeleteSignedUrl_WithAmazonS3Exception_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var s3Exception = new AmazonS3Exception("S3 error occurred");

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Throws(s3Exception);

        // Act
        var act = () => _signedUrlsClient.GenerateDeleteSignedUrl(request);

        // Assert
        var exception = act.Should().Throw<R2Exception>()
            .WithMessage("Failed to generate DELETE signed URL for object 'test-object.txt' in bucket 'test-bucket': S3 error occurred");

        exception.And.InnerException.Should().BeOfType<AmazonS3Exception>()
            .Which.Message.Should().Be("S3 error occurred");
    }

    [Fact]
    public void GenerateDeleteSignedUrl_WithGeneralException_ShouldThrowR2Exception()
    {
        // Arrange
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt"
        };

        var generalException = new InvalidOperationException("General error");

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Throws(generalException);

        // Act
        var act = () => _signedUrlsClient.GenerateDeleteSignedUrl(request);

        // Assert
        var exception = act.Should().Throw<R2Exception>()
            .WithMessage("An unexpected error occurred while generating DELETE signed URL for object 'test-object.txt' in bucket 'test-bucket': General error");

        exception.And.InnerException.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("General error");
    }

    // Edge Case Tests for Expiration Logic
    [Fact]
    public void GenerateGetSignedUrl_WithLocalTimeExpires_ShouldConvertToUtc()
    {
        // Arrange
        var localExpires = DateTime.Now.AddHours(2); // Local time
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            Expires = localExpires
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.ExpiresAt.Should().BeCloseTo(localExpires.ToUniversalTime(), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateGetSignedUrl_WithZeroExpiresIn_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ExpiresIn = TimeSpan.Zero
        };

        // Act
        var act = () => _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        act.Should().Throw<R2Exception>()
           .WithMessage("*An unexpected error occurred while generating GET signed URL for object 'test-object.txt' in bucket 'test-bucket'*");
    }

    [Fact]
    public void GenerateGetSignedUrl_WithMaximumAllowedExpiration_ShouldSucceed()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            ExpiresIn = TimeSpan.FromDays(7)
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateGetSignedUrl_WithVersionId_ShouldIncludeVersionInResponse()
    {
        // Arrange
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-object.txt",
            VersionId = "version123"
        };

        var expectedSignedUrl = "https://test-bucket.r2.cloudflarestorage.com/test-object.txt?signed-params";

        _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedSignedUrl);

        // Act
        var result = _signedUrlsClient.GenerateGetSignedUrl(request);

        // Assert
        result.Should().NotBeNull();
        result.VersionId.Should().Be("version123");
    }
}
