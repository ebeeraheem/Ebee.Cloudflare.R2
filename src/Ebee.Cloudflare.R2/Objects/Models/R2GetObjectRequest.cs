namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Request to get an object from an R2 bucket.
/// </summary>
public class R2GetObjectRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket containing the object.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the key (name/path) of the object to retrieve.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the object to retrieve.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets the range of bytes to retrieve (e.g., "bytes=0-1023").
    /// </summary>
    public string? Range { get; set; }

    /// <summary>
    /// Gets or sets the if-match condition.
    /// </summary>
    public string? IfMatch { get; set; }

    /// <summary>
    /// Gets or sets the if-none-match condition.
    /// </summary>
    public string? IfNoneMatch { get; set; }

    /// <summary>
    /// Gets or sets the if-modified-since condition.
    /// </summary>
    public DateTime? IfModifiedSince { get; set; }

    /// <summary>
    /// Gets or sets the if-unmodified-since condition.
    /// </summary>
    public DateTime? IfUnmodifiedSince { get; set; }
}