using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.Objects.Models;
using System.Globalization;

namespace Ebee.Cloudflare.R2.Objects;

/// <summary>
/// Client for R2 object operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ObjectsClient"/> class.
/// </remarks>
/// <param name="s3Client">The S3 client instance.</param>
public class ObjectsClient(IAmazonS3 s3Client) : IObjectsClient
{
    private readonly IAmazonS3 _s3Client = s3Client
        ?? throw new ArgumentNullException(nameof(s3Client));

    /// <inheritdoc />
    public async Task<R2ListObjectsResponse> ListObjectsAsync(
        R2ListObjectsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);

        try
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = request.BucketName,
                Prefix = request.Prefix,
                Delimiter = request.Delimiter,
                MaxKeys = request.MaxKeys,
                ContinuationToken = request.ContinuationToken,
                FetchOwner = request.FetchOwner,
                StartAfter = request.StartAfter
            };

            var response = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            return new R2ListObjectsResponse
            {
                BucketName = request.BucketName,
                Objects = [.. response.S3Objects
                    .Select(obj => new R2ObjectInfoResponse
                    {
                        Key = obj.Key,
                        Size = obj.Size,
                        LastModified = obj.LastModified,
                        ETag = obj.ETag,
                        StorageClass = obj.StorageClass,
                        Owner = obj.Owner?.DisplayName
                    })],
                Prefix = response.Prefix,
                Delimiter = response.Delimiter,
                MaxKeys = response.MaxKeys,
                IsTruncated = response.IsTruncated,
                NextContinuationToken = response.NextContinuationToken,
                KeyCount = response.KeyCount,
                CommonPrefixes = [.. response.CommonPrefixes]
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to list objects in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while listing objects in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2GetObjectResponse> GetObjectAsync(
        R2GetObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                VersionId = request.VersionId
            };

            if (!string.IsNullOrEmpty(request.Range))
                getRequest.ByteRange = new ByteRange(request.Range);

            if (!string.IsNullOrEmpty(request.IfMatch))
                getRequest.EtagToMatch = request.IfMatch;

            if (!string.IsNullOrEmpty(request.IfNoneMatch))
                getRequest.EtagToNotMatch = request.IfNoneMatch;

            if (request.IfModifiedSince.HasValue)
                getRequest.ModifiedSinceDateUtc = request.IfModifiedSince.Value;

            if (request.IfUnmodifiedSince.HasValue)
                getRequest.UnmodifiedSinceDateUtc = request.IfUnmodifiedSince.Value;

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);

            // Read content as byte array for the response
            byte[] contentBytes;
            using (var memoryStream = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                contentBytes = memoryStream.ToArray();
            }

            return new R2GetObjectResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                ContentBytes = contentBytes,
                ContentStream = new MemoryStream(contentBytes),
                ContentType = response.Headers.ContentType,
                ContentLength = response.Headers.ContentLength,
                ETag = response.ETag,
                LastModified = response.LastModified,
                VersionId = response.VersionId,
                Metadata = ConvertMetadataCollectionToDictionary(response.Metadata),
                CacheControl = response.Headers.CacheControl,
                ContentDisposition = response.Headers.ContentDisposition,
                ContentEncoding = response.Headers.ContentEncoding,
                Expires = ParseExpiresStringToDateTime(response.ExpiresString),
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            throw new R2Exception($"Object '{request.Key}' does not exist in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to get object '{request.Key}' from bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while getting object '{request.Key}' from bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2PutObjectResponse> PutObjectAsync(
        R2PutObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        // Validate that exactly one content source is provided
        var contentSources = (request.ContentStream, request.ContentBytes, request.FilePath) switch
        {
            (not null, null, null) => 1,
            (null, not null, null) => 1,
            (null, null, not null) => 1,
            (null, null, null) => 0,
            _ => 2 // More than one
        };

        if (contentSources == 0)
            throw new ArgumentException("Exactly one content source must be provided. Set either ContentStream, ContentBytes, or FilePath.");

        if (contentSources > 1)
            throw new ArgumentException("Only one content source can be provided. Set either ContentStream, ContentBytes, or FilePath, but not multiple.");

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                ContentType = request.ContentType,
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true,
                ServerSideEncryptionMethod = !string.IsNullOrEmpty(request.ServerSideEncryption)
                    ? ServerSideEncryptionMethod.FindValue(request.ServerSideEncryption)
                    : null,
                StorageClass = !string.IsNullOrEmpty(request.StorageClass)
                    ? S3StorageClass.FindValue(request.StorageClass)
                    : null
            };

            // Set content source
            if (request.ContentStream is not null)
            {
                putRequest.InputStream = request.ContentStream;
            }
            else if (request.ContentBytes is not null)
            {
                putRequest.InputStream = new MemoryStream(request.ContentBytes);
            }
            else if (!string.IsNullOrEmpty(request.FilePath))
            {
                putRequest.FilePath = request.FilePath;
            }

            // Add metadata
            foreach (var metadata in request.Metadata)
            {
                putRequest.Metadata.Add(metadata.Key, metadata.Value);
            }

            // Set server-side encryption with customer-provided keys
            if (!string.IsNullOrEmpty(request.SSECustomerAlgorithm))
            {
                putRequest.ServerSideEncryptionCustomerMethod = ServerSideEncryptionCustomerMethod.FindValue(request.SSECustomerAlgorithm);
                putRequest.ServerSideEncryptionCustomerProvidedKey = request.SSECustomerKey;
                putRequest.ServerSideEncryptionCustomerProvidedKeyMD5 = request.SSECustomerKeyMD5;
            }

            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            return new R2PutObjectResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                ETag = response.ETag,
                VersionId = response.VersionId,
                ServerSideEncryption = response.ServerSideEncryptionMethod?.Value,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to put object '{request.Key}' to bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while putting object '{request.Key}' to bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2DeleteObjectResponse> DeleteObjectAsync(
        R2DeleteObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                VersionId = request.VersionId,
            };

            if (!string.IsNullOrEmpty(request.ExpectedBucketOwner))
                deleteRequest.ExpectedBucketOwner = request.ExpectedBucketOwner;

            var response = await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);

            return new R2DeleteObjectResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                VersionId = response.VersionId,
                DeleteMarker = GetDeleteMarkerFlag(response),
                DeletedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            throw new R2Exception($"Object '{request.Key}' does not exist in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to delete object '{request.Key}' from bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while deleting object '{request.Key}' from bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2GetObjectMetadataResponse> GetObjectMetadataAsync(
        R2GetObjectMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                VersionId = request.VersionId
            };

            if (!string.IsNullOrEmpty(request.IfMatch))
                metadataRequest.EtagToMatch = request.IfMatch;

            if (!string.IsNullOrEmpty(request.IfNoneMatch))
                metadataRequest.EtagToNotMatch = request.IfNoneMatch;

            if (request.IfModifiedSince.HasValue)
                metadataRequest.ModifiedSinceDateUtc = request.IfModifiedSince.Value;

            if (request.IfUnmodifiedSince.HasValue)
                metadataRequest.UnmodifiedSinceDateUtc = request.IfUnmodifiedSince.Value;

            var response = await _s3Client.GetObjectMetadataAsync(metadataRequest, cancellationToken);

            return new R2GetObjectMetadataResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                ContentType = response.Headers.ContentType,
                ContentLength = response.Headers.ContentLength,
                ETag = response.ETag,
                LastModified = response.LastModified,
                VersionId = response.VersionId,
                Metadata = ConvertMetadataCollectionToDictionary(response.Metadata),
                CacheControl = response.Headers.CacheControl,
                ContentDisposition = response.Headers.ContentDisposition,
                ContentEncoding = response.Headers.ContentEncoding,
                Expires = response.ExpiresString,
                StorageClass = response.StorageClass,
                ServerSideEncryption = response.ServerSideEncryptionMethod
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            throw new R2Exception($"Object '{request.Key}' does not exist in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to get metadata for object '{request.Key}' from bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while getting metadata for object '{request.Key}' from bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2CopyObjectResponse> CopyObjectAsync(
        R2CopyObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.SourceBucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.SourceKey);
        ArgumentException.ThrowIfNullOrEmpty(request.DestinationBucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.DestinationKey);

        try
        {
            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = request.SourceBucketName,
                SourceKey = request.SourceKey,
                DestinationBucket = request.DestinationBucketName,
                DestinationKey = request.DestinationKey,
                SourceVersionId = request.SourceVersionId,
                MetadataDirective = GetS3MetadataDirective(request.MetadataDirective),
                ContentType = request.ContentType,

                ServerSideEncryptionMethod = !string.IsNullOrEmpty(request.ServerSideEncryption)
                    ? ServerSideEncryptionMethod.FindValue(request.ServerSideEncryption)
                    : null,
                StorageClass = !string.IsNullOrEmpty(request.StorageClass)
                    ? S3StorageClass.FindValue(request.StorageClass)
                    : null
            };
            
            // Add metadata
            foreach (var metadata in request.Metadata)
            {
                copyRequest.Metadata.Add(metadata.Key, metadata.Value);
            }

            var response = await _s3Client.CopyObjectAsync(copyRequest, cancellationToken);

            return new R2CopyObjectResponse
            {
                SourceBucketName = request.SourceBucketName,
                SourceKey = request.SourceKey,
                DestinationBucketName = request.DestinationBucketName,
                DestinationKey = request.DestinationKey,
                ETag = response.ETag,
                VersionId = response.VersionId,
                LastModified = ParseLastModifiedDate(response.LastModified),
                CopiedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Source bucket '{request.SourceBucketName}' or destination bucket '{request.DestinationBucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            throw new R2Exception($"Source object '{request.SourceKey}' does not exist in bucket '{request.SourceBucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to copy object from '{request.SourceBucketName}/{request.SourceKey}' to '{request.DestinationBucketName}/{request.DestinationKey}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while copying object from '{request.SourceBucketName}/{request.SourceKey}' to '{request.DestinationBucketName}/{request.DestinationKey}': {ex.Message}", ex);
        }
    }

    private static DateTime ParseLastModifiedDate(string lastModifiedString)
    {
        if (string.IsNullOrWhiteSpace(lastModifiedString))
            return DateTime.UtcNow;

        return DateTime.TryParse(
            lastModifiedString,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var lastModified)
            ? lastModified : DateTime.UtcNow;
    }

    private static DateTime? ParseExpiresStringToDateTime(string? expiresString)
    {
        if (string.IsNullOrWhiteSpace(expiresString))
            return null;

        // Try parsing using invariant culture and roundtrip kind
        return DateTime.TryParse(
            expiresString,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var expires)
            ? expires : null;
    }

    private static S3MetadataDirective GetS3MetadataDirective(string? directive) =>
        directive?.ToLower() switch
        {
            "copy" => S3MetadataDirective.COPY,
            "replace" => S3MetadataDirective.REPLACE,
            _ => S3MetadataDirective.COPY // Default to COPY if not specified or unrecognized
        };

    private static bool GetDeleteMarkerFlag(DeleteObjectResponse response)
    {
        return !string.IsNullOrEmpty(response.DeleteMarker) &&
                bool.TryParse(response.DeleteMarker, out var deleteMarker) &&
                deleteMarker;
    }

    private static Dictionary<string, string> ConvertMetadataCollectionToDictionary(MetadataCollection metadataCollection)
    {
        return metadataCollection.Keys.ToDictionary(key => key, key => metadataCollection[key]);
    }
}
