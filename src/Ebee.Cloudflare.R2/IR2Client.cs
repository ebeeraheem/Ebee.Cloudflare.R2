using Ebee.Cloudflare.R2.Buckets;

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
}
