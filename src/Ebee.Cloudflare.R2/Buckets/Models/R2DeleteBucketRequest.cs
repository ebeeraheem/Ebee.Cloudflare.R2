namespace Ebee.Cloudflare.R2.Buckets.Models;

/// <summary>
/// Request to delete an R2 bucket.
/// </summary>
public class R2DeleteBucketRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket to delete.
    /// </summary>
    public required string BucketName { get; set; }
}