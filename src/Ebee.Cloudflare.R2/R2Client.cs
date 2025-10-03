using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Objects;

namespace Ebee.Cloudflare.R2;

/// <summary>
/// Main client for Cloudflare R2 operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="R2Client"/> class.
/// </remarks>
/// <param name="bucketsClient">The buckets client.</param>
/// <param name="objectsClient">The objects client.</param>
public class R2Client(IBucketsClient bucketsClient, IObjectsClient objectsClient) : IR2Client
{
    /// <inheritdoc />
    public IBucketsClient Buckets { get; } = bucketsClient
        ?? throw new ArgumentNullException(nameof(bucketsClient));

    /// <inheritdoc />
    public IObjectsClient Objects { get; } = objectsClient
        ?? throw new ArgumentNullException(nameof(objectsClient));
}