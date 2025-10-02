using Amazon.S3;
using Amazon.S3.Model;
using Ebee.Cloudflare.R2.Buckets.Models;
using ListBucketsResponse = Ebee.Cloudflare.R2.Buckets.Models.ListBucketsResponse;

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
    public async Task<ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListBucketsRequest();
            var response = await _s3Client.ListBucketsAsync(request, cancellationToken);

            return new ListBucketsResponse
            {
                Buckets = [.. response.Buckets
                .Select(bucket => new BucketInfoResponse
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
}
