using Ebee.Cloudflare.R2.Buckets.Models;

namespace Ebee.Cloudflare.R2.Buckets;

/// <summary>
/// Interface for R2 bucket operations.
/// </summary>
public interface IBucketsClient
{
    /// <summary>
    /// Lists all buckets in the R2 account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the list of buckets.</returns>
    Task<R2ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new bucket in the R2 account.
    /// </summary>
    /// <param name="request">The create bucket request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the create bucket response.</returns>
    /// <exception cref="R2Exception">Thrown when the bucket creation fails.</exception>
    /// <example>
    /// <code>
    /// var request = new CreateBucketRequest { BucketName = "my-bucket" };
    /// var response = await bucketsClient.CreateBucketAsync(request);
    /// </code>
    /// </example>
    Task<R2CreateBucketResponse> CreateBucketAsync(
        R2CreateBucketRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bucket from the R2 account.
    /// </summary>
    /// <param name="request">The delete bucket request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the delete bucket response.</returns>
    /// <exception cref="R2Exception">Thrown when the bucket deletion fails.</exception>
    /// <example>
    /// <code>
    /// var request = new DeleteBucketRequest { BucketName = "my-bucket" };
    /// var response = await bucketsClient.DeleteBucketAsync(request);
    /// </code>
    /// </example>
    Task<R2DeleteBucketResponse> DeleteBucketAsync(
        R2DeleteBucketRequest request,
        CancellationToken cancellationToken = default);
}
