namespace Ebee.Cloudflare.R2.SignedUrls.Models;

/// <summary>
/// Base request to generate a signed URL for an R2 object.
/// </summary>
public abstract class R2GenerateSignedUrlRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket containing the object.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the key (name/path) of the object.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the expiration time for the signed URL.
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Gets or sets the expiration duration for the signed URL.
    /// </summary>
    public TimeSpan? ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the object.
    /// </summary>
    public string? VersionId { get; set; }
}
