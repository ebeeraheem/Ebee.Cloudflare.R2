namespace Ebee.Cloudflare.R2.Buckets.Models;

/// <summary>
/// Response from deleting an R2 bucket.
/// </summary>
public class R2DeleteBucketResponse
{
    /// <summary>
    /// Gets or sets the name of the deleted bucket.
    /// </summary>
    public required string BucketName { get; set; }
}
