using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.MultipartUploads.Models;

namespace Ebee.Cloudflare.R2.MultipartUploads;

/// <summary>
/// Client for R2 multipart upload operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MultipartUploadsClient"/> class.
/// </remarks>
/// <param name="s3Client">The S3 client instance.</param>
public class MultipartUploadsClient(IAmazonS3 s3Client) : IMultipartUploadsClient
{
    private readonly IAmazonS3 _s3Client = s3Client
        ?? throw new ArgumentNullException(nameof(s3Client));

    /// <inheritdoc />
    public async Task<R2InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(
        R2InitiateMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);

        try
        {
            var initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
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
                initiateRequest.Metadata.Add(metadata.Key, metadata.Value);
            }

            // Set headers
            if (!string.IsNullOrEmpty(request.CacheControl))
                initiateRequest.Headers.CacheControl = request.CacheControl;

            if (!string.IsNullOrEmpty(request.ContentDisposition))
                initiateRequest.Headers.ContentDisposition = request.ContentDisposition;

            if (!string.IsNullOrEmpty(request.ContentEncoding))
                initiateRequest.Headers.ContentEncoding = request.ContentEncoding;

            if (request.Expires.HasValue)
                initiateRequest.Headers.Expires = request.Expires.Value;

            // Set server-side encryption with customer-provided keys
            if (!string.IsNullOrEmpty(request.SSECustomerAlgorithm))
            {
                initiateRequest.ServerSideEncryptionCustomerMethod =
                    ServerSideEncryptionCustomerMethod.FindValue(request.SSECustomerAlgorithm);
                initiateRequest.ServerSideEncryptionCustomerProvidedKey = request.SSECustomerKey;
                initiateRequest.ServerSideEncryptionCustomerProvidedKeyMD5 = request.SSECustomerKeyMD5;
            }

            var response = await _s3Client.InitiateMultipartUploadAsync(
                initiateRequest, cancellationToken);

            return new R2InitiateMultipartUploadResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = response.UploadId,
                ServerSideEncryption = response.ServerSideEncryptionMethod?.Value,
                InitiatedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to initiate multipart upload for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while initiating multipart upload for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2UploadPartResponse> UploadPartAsync(
        R2UploadPartRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);
        ArgumentException.ThrowIfNullOrEmpty(request.UploadId);

        if (request.PartNumber < 1 || request.PartNumber > 10000)
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.PartNumber,
                "Part number must be between 1 and 10000.");

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
            var uploadRequest = new UploadPartRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId,
                PartNumber = request.PartNumber,
                MD5Digest = request.ContentMD5,
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            // Set content source
            if (request.ContentStream is not null)
            {
                uploadRequest.InputStream = request.ContentStream;
            }
            else if (request.ContentBytes is not null)
            {
                uploadRequest.InputStream = new MemoryStream(request.ContentBytes);
            }
            else if (!string.IsNullOrEmpty(request.FilePath))
            {
                uploadRequest.FilePath = request.FilePath;
            }

            // Set server-side encryption with customer-provided keys
            if (!string.IsNullOrEmpty(request.SSECustomerAlgorithm))
            {
                uploadRequest.ServerSideEncryptionCustomerMethod =
                    ServerSideEncryptionCustomerMethod.FindValue(request.SSECustomerAlgorithm);
                uploadRequest.ServerSideEncryptionCustomerProvidedKey = request.SSECustomerKey;
                uploadRequest.ServerSideEncryptionCustomerProvidedKeyMD5 = request.SSECustomerKeyMD5;
            }

            var response = await _s3Client.UploadPartAsync(uploadRequest, cancellationToken);

            return new R2UploadPartResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId,
                PartNumber = request.PartNumber,
                ETag = response.ETag,
                ServerSideEncryption = response.ServerSideEncryptionMethod?.Value,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchUpload")
        {
            throw new R2Exception($"Multipart upload '{request.UploadId}' does not exist for object '{request.Key}' in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to upload part {request.PartNumber} for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while uploading part {request.PartNumber} for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(
        R2CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);
        ArgumentException.ThrowIfNullOrEmpty(request.UploadId);

        if (request.Parts.Count == 0)
            throw new ArgumentException("At least one part must be provided to complete the multipart upload.");

        try
        {
            var completeRequest = new CompleteMultipartUploadRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId
            };

            // Add parts
            foreach (var part in request.Parts)
            {
                completeRequest.AddPartETags(new PartETag(part.PartNumber, part.ETag));
            }

            var response = await _s3Client.CompleteMultipartUploadAsync(
                completeRequest, cancellationToken);

            return new R2CompleteMultipartUploadResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                ETag = response.ETag,
                Location = response.Location,
                VersionId = response.VersionId,
                ServerSideEncryption = response.ServerSideEncryptionMethod?.Value,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchUpload")
        {
            throw new R2Exception($"Multipart upload '{request.UploadId}' does not exist for object '{request.Key}' in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidPart")
        {
            throw new R2Exception($"One or more parts are invalid for multipart upload '{request.UploadId}' of object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to complete multipart upload for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while completing multipart upload for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2AbortMultipartUploadResponse> AbortMultipartUploadAsync(
        R2AbortMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);
        ArgumentException.ThrowIfNullOrEmpty(request.UploadId);

        try
        {
            var abortRequest = new AbortMultipartUploadRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId,
                ExpectedBucketOwner = request.ExpectedBucketOwner
            };

            await _s3Client.AbortMultipartUploadAsync(abortRequest, cancellationToken);

            return new R2AbortMultipartUploadResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId,
                AbortedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchUpload")
        {
            throw new R2Exception($"Multipart upload '{request.UploadId}' does not exist for object '{request.Key}' in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to abort multipart upload '{request.UploadId}' for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while aborting multipart upload '{request.UploadId}' for object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2ListPartsResponse> ListPartsAsync(
        R2ListPartsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);
        ArgumentException.ThrowIfNullOrEmpty(request.Key);
        ArgumentException.ThrowIfNullOrEmpty(request.UploadId);

        try
        {
            var listRequest = new ListPartsRequest
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId,
                MaxParts = request.MaxParts,
                PartNumberMarker = request.PartNumberMarker?.ToString(),
                ExpectedBucketOwner = request.ExpectedBucketOwner
            };

            var response = await _s3Client.ListPartsAsync(listRequest, cancellationToken);

            return new R2ListPartsResponse
            {
                BucketName = request.BucketName,
                Key = request.Key,
                UploadId = request.UploadId,
                Parts = [.. response.Parts
                    .Select(part => new R2PartInfoResponse
                    {
                        PartNumber = part.PartNumber,
                        ETag = part.ETag,
                        Size = part.Size,
                        LastModified = part.LastModified
                    })],
                MaxParts = response.MaxParts,
                IsTruncated = response.IsTruncated,
                NextPartNumberMarker = response.NextPartNumberMarker,
                StorageClass = response.StorageClass,
                Owner = response.Owner?.DisplayName
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchUpload")
        {
            throw new R2Exception($"Multipart upload '{request.UploadId}' does not exist for object '{request.Key}' in bucket '{request.BucketName}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to list parts for multipart upload '{request.UploadId}' of object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while listing parts for multipart upload '{request.UploadId}' of object '{request.Key}' in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2ListMultipartUploadsResponse> ListMultipartUploadsAsync(
        R2ListMultipartUploadsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.BucketName);

        try
        {
            var listRequest = new ListMultipartUploadsRequest
            {
                BucketName = request.BucketName,
                Prefix = request.Prefix,
                Delimiter = request.Delimiter,
                MaxUploads = request.MaxUploads,
                KeyMarker = request.KeyMarker,
                UploadIdMarker = request.UploadIdMarker,
                ExpectedBucketOwner = request.ExpectedBucketOwner
            };

            var response = await _s3Client.ListMultipartUploadsAsync(listRequest, cancellationToken);
            var multipartUploads = response.MultipartUploads ?? [];

            return new R2ListMultipartUploadsResponse
            {
                BucketName = request.BucketName,
                Uploads = [.. multipartUploads
                    .Select(upload => new R2MultipartUploadInfoResponse
                    {
                        Key = upload.Key,
                        UploadId = upload.UploadId,
                        Initiated = upload.Initiated,
                        StorageClass = upload.StorageClass,
                        Owner = upload.Owner?.DisplayName
                    })],
                Prefix = response.Prefix,
                Delimiter = response.Delimiter,
                MaxUploads = response.MaxUploads,
                IsTruncated = response.IsTruncated,
                NextKeyMarker = response.NextKeyMarker,
                NextUploadIdMarker = response.NextUploadIdMarker,
                CommonPrefixes = response.CommonPrefixes,
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to list multipart uploads in bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while listing multipart uploads in bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }
}
