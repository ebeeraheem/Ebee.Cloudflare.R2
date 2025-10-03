using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.Buckets.Models;

namespace Ebee.Cloudflare.R2.Buckets;

/// <summary>
/// Client for R2 bucket operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BucketsClient"/> class.
/// </remarks>
/// <param name="s3Client">The S3 client instance.</param>
public class BucketsClient(IAmazonS3 s3Client) : IBucketsClient
{
    private readonly IAmazonS3 _s3Client = s3Client
        ?? throw new ArgumentNullException(nameof(s3Client));

    /// <inheritdoc />
    public async Task<R2ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListBucketsRequest();
            var response = await _s3Client.ListBucketsAsync(request, cancellationToken);

            return new R2ListBucketsResponse
            {
                Buckets = [.. response.Buckets
                .Select(bucket => new R2BucketInfoResponse
                {
                    Name = bucket.BucketName,
                    CreationDate = bucket.CreationDate
                })],
                Owner = response.Owner?.DisplayName
            };
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to list buckets: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while listing buckets: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2CreateBucketResponse> CreateBucketAsync(
        R2CreateBucketRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = request.BucketName
            };

            var response = await _s3Client.PutBucketAsync(putBucketRequest, cancellationToken);

            return new R2CreateBucketResponse
            {
                BucketName = request.BucketName,
                Location = response.Location,
                CreationDate = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyExists")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' already exists.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' already owned by you.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidBucketName")
        {
            throw new R2Exception($"Invalid bucket name '{request.BucketName}': {ex.Message}", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to create bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while creating bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<R2DeleteBucketResponse> DeleteBucketAsync(
        R2DeleteBucketRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _s3Client.DeleteBucketAsync(request.BucketName, cancellationToken);

            return new R2DeleteBucketResponse
            {
                BucketName = request.BucketName
            };
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' does not exist.", ex);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketNotEmpty")
        {
            throw new R2Exception($"Bucket '{request.BucketName}' is not empty and cannot be deleted.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new R2Exception($"Failed to delete bucket '{request.BucketName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new R2Exception($"An unexpected error occurred while deleting bucket '{request.BucketName}': {ex.Message}", ex);
        }
    }
}