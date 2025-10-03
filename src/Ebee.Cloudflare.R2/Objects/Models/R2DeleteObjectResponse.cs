namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Response from deleting an object from an R2 bucket.
/// </summary>
public class R2DeleteObjectResponse
{
    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the deleted object key.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the deleted object.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets whether a delete marker was created.
    /// </summary>
    public bool DeleteMarker { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp.
    /// </summary>
    public DateTime DeletedAt { get; set; }
}