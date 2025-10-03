namespace Ebee.Cloudflare.R2.Buckets.Models;

/// <summary>
/// Request to create a new R2 bucket.
/// </summary>
public class CreateBucketRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket to create.
    /// </summary>
    public required string BucketName { get; set; }
}
