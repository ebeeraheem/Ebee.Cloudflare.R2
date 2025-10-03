namespace Ebee.Cloudflare.R2.Buckets.Models;

/// <summary>
/// Response from creating an R2 bucket.
/// </summary>
public class CreateBucketResponse
{
    /// <summary>
    /// Gets or sets the name of the created bucket.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the location where the bucket was created.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the bucket.
    /// </summary>
    public DateTime CreationDate { get; set; }
}
