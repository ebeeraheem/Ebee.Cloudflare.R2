namespace Ebee.Cloudflare.R2.Buckets.Models;

/// <summary>
/// Represents information about an R2 bucket.
/// </summary>
public class BucketInfoResponse
{
    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the bucket creation date.
    /// </summary>
    public DateTime CreationDate { get; set; }
}
