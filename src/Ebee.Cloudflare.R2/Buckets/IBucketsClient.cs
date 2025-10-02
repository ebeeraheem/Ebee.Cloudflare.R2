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
    Task<ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default);
}
