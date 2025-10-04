namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Response containing object metadata from an R2 bucket.
/// </summary>
public class R2GetObjectMetadataResponse
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
    /// Gets or sets the content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content length.
    /// </summary>
    public long ContentLength { get; set; }

    /// <summary>
    /// Gets or sets the ETag.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets the version ID.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata.
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
    /// Gets or sets the expires header.
    /// </summary>
    public string? Expires { get; set; }

    /// <summary>
    /// Gets or sets the storage class.
    /// </summary>
    public string? StorageClass { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method.
    /// </summary>
    public string? ServerSideEncryption { get; set; }
}