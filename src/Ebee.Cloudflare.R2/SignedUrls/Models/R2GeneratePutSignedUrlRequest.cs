namespace Ebee.Cloudflare.R2.SignedUrls.Models;

/// <summary>
/// Request to generate a signed URL for uploading an object to R2.
/// </summary>
public class R2GeneratePutSignedUrlRequest : R2GenerateSignedUrlRequest
{
    /// <summary>
    /// Gets or sets the content type for the upload.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata for the upload.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets the cache control header.
    /// </summary>
    public string? CacheControl { get; set; }

    /// <summary>
    /// Gets or sets the content disposition header.
    /// </summary>
    public string? ContentDisposition { get; set; }

    /// <summary>
    /// Gets or sets the content encoding header.
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets the storage class.
    /// </summary>
    public string? StorageClass { get; set; }
}
