using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.SignedUrls.Models;

namespace Ebee.Cloudflare.R2.SignedUrls;

/// <summary>
/// Client for R2 signed URL operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SignedUrlsClient"/> class.
/// </remarks>
/// <param name="s3Client">The S3 client instance.</param>
public class SignedUrlsClient(IAmazonS3 s3Client) : ISignedUrlsClient
{
    private readonly IAmazonS3 _s3Client = s3Client
        ?? throw new ArgumentNullException(nameof(s3Client));

    private static readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);
    private static readonly TimeSpan _maxExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public R2SignedUrlResponse GenerateGetSignedUrl(R2GenerateGetSignedUrlRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var expires = CalculateExpiration(request.Expires, request.ExpiresIn);

            var getPreSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                Expires = expires,
                Verb = HttpVerb.GET,
                VersionId = request.VersionId
            };

            // Add response headers if specified
            if (!string.IsNullOrEmpty(request.ResponseContentType))
                getPreSignedUrlRequest.ResponseHeaderOverrides.ContentType = request.ResponseContentType;

            if (!string.IsNullOrEmpty(request.ResponseContentDisposition))
                getPreSignedUrlRequest.ResponseHeaderOverrides.ContentDisposition = request.ResponseContentDisposition;

            if (!string.IsNullOrEmpty(request.ResponseCacheControl))
                getPreSignedUrlRequest.ResponseHeaderOverrides.CacheControl = request.ResponseCacheControl;

            if (request.ResponseExpires.HasValue)
                getPreSignedUrlRequest.ResponseHeaderOverrides.Expires = request.ResponseExpires.Value.ToString("R");

            var signedUrl = _s3Client.GetPreSignedURL(getPreSignedUrlRequest);

            return new R2SignedUrlResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                SignedUrl = signedUrl,
                HttpMethod = "GET",
                ExpiresAt = expires,
                VersionId = request.VersionId,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to generate GET signed URL for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while generating GET signed URL for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public R2SignedUrlResponse GeneratePutSignedUrl(R2GeneratePutSignedUrlRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var expires = CalculateExpiration(request.Expires, request.ExpiresIn);

            var getPreSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                Expires = expires,
                Verb = HttpVerb.PUT,
                VersionId = request.VersionId,
                ContentType = request.ContentType
            };

            // Add headers if specified
            if (!string.IsNullOrEmpty(request.CacheControl))
                getPreSignedUrlRequest.Headers.CacheControl = request.CacheControl;

            if (!string.IsNullOrEmpty(request.ContentDisposition))
                getPreSignedUrlRequest.Headers.ContentDisposition = request.ContentDisposition;

            if (!string.IsNullOrEmpty(request.ContentEncoding))
                getPreSignedUrlRequest.Headers.ContentEncoding = request.ContentEncoding;

            if (!string.IsNullOrEmpty(request.ServerSideEncryption))
            {
                getPreSignedUrlRequest.ServerSideEncryptionMethod =
                    ServerSideEncryptionMethod.FindValue(request.ServerSideEncryption);
            }

            // Add metadata
            foreach (var metadata in request.Metadata)
            {
                getPreSignedUrlRequest.Metadata.Add(metadata.Key, metadata.Value);
            }

            var signedUrl = _s3Client.GetPreSignedURL(getPreSignedUrlRequest);

            return new R2SignedUrlResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                SignedUrl = signedUrl,
                HttpMethod = "PUT",
                ExpiresAt = expires,
                VersionId = request.VersionId,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to generate PUT signed URL for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while generating PUT signed URL for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public R2SignedUrlResponse GenerateDeleteSignedUrl(R2GenerateDeleteSignedUrlRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var expires = CalculateExpiration(request.Expires, request.ExpiresIn);

            var getPreSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                Expires = expires,
                Verb = HttpVerb.DELETE,
                VersionId = request.VersionId
            };

            if (!string.IsNullOrEmpty(request.ExpectedBucketOwner))
                getPreSignedUrlRequest.Headers["x-amz-expected-bucket-owner"] = request.ExpectedBucketOwner;

            if (request.BypassGovernanceRetention)
                getPreSignedUrlRequest.Headers["x-amz-bypass-governance-retention"] = "true";

            var signedUrl = _s3Client.GetPreSignedURL(getPreSignedUrlRequest);

            return new R2SignedUrlResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                SignedUrl = signedUrl,
                HttpMethod = "DELETE",
                ExpiresAt = expires,
                VersionId = request.VersionId,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to generate DELETE signed URL for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while generating DELETE signed URL for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    private static DateTime CalculateExpiration(DateTime? expires, TimeSpan? expiresIn)
    {
        var now = DateTime.UtcNow;

        // Use explicit expires if provided
        if (expires.HasValue)
        {
            var requestedExpires = expires.Value.Kind == DateTimeKind.Utc
                ? expires.Value
                : expires.Value.ToUniversalTime();

            if (requestedExpires <= now)
                throw new ArgumentOutOfRangeException(nameof(expires), "Expiration time must be in the future.");

            var requestedDuration = requestedExpires - now;
            if (requestedDuration > _maxExpiration)
                throw new ArgumentOutOfRangeException(nameof(expires), $"Expiration time cannot be more than {_maxExpiration.TotalDays} days in the future.");

            return requestedExpires;
        }

        // Use expiresIn if provided
        if (expiresIn.HasValue)
        {
            if (expiresIn.Value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(expiresIn), "Expiration duration must be positive.");

            if (expiresIn.Value > _maxExpiration)
                throw new ArgumentOutOfRangeException(nameof(expiresIn), $"Expiration duration cannot be more than {_maxExpiration.TotalDays} days.");

            return now.Add(expiresIn.Value);
        }

        // Use default expiration
        return now.Add(_defaultExpiration);
    }
}
