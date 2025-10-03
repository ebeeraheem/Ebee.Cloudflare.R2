namespace Ebee.Cloudflare.R2.SignedUrls.Models;

/// <summary>
/// Response containing a signed URL for an R2 object.
/// </summary>
public class R2SignedUrlResponse
{
    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the object key.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the signed URL.
    /// </summary>
    public required string SignedUrl { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method for which the URL is signed.
    /// </summary>
    public required string HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the signed URL.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the object.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets when the signed URL was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
