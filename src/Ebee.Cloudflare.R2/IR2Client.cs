using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.SignedUrls;

namespace Ebee.Cloudflare.R2;

/// <summary>
/// Interface for the main R2 client.
/// </summary>
public interface IR2Client
{
    /// <summary>
    /// Gets the buckets client for bucket operations.
    /// </summary>
    IBucketsClient Buckets { get; }

    /// <summary>
    /// Gets the objects client for object operations.
    /// </summary>
    IObjectsClient Objects { get; }

    /// <summary>
    /// Gets the signed URLs client for signed URL operations.
    /// </summary>
    ISignedUrlsClient SignedUrls { get; }
}
