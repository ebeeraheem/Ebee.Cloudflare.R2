using Ebee.Cloudflare.R2.Buckets;

namespace Ebee.Cloudflare.R2;

/// <summary>
/// Main client for Cloudflare R2 operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="R2Client"/> class.
/// </remarks>
/// <param name="bucketsClient">The buckets client.</param>
public class R2Client(IBucketsClient bucketsClient) : IR2Client
{

    /// <inheritdoc />
    public IBucketsClient Buckets { get; } = bucketsClient
        ?? throw new ArgumentNullException(nameof(bucketsClient));
}