namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Request to copy an object within or between R2 buckets.
/// </summary>
public class R2CopyObjectRequest
{
    /// <summary>
    /// Gets or sets the source bucket name.
    /// </summary>
    public required string SourceBucketName { get; set; }

    /// <summary>
    /// Gets or sets the source object key.
    /// </summary>
    public required string SourceKey { get; set; }

    /// <summary>
    /// Gets or sets the destination bucket name.
    /// </summary>
    public required string DestinationBucketName { get; set; }

    /// <summary>
    /// Gets or sets the destination object key.
    /// </summary>
    public required string DestinationKey { get; set; }

    /// <summary>
    /// Gets or sets the source version ID.
    /// </summary>
    public string? SourceVersionId { get; set; }

    /// <summary>
    /// Gets or sets the metadata directive (COPY or REPLACE).
    /// </summary>
    public string? MetadataDirective { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata for the copied object.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets the content type for the copied object.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets the storage class.
    /// </summary>
    public string? StorageClass { get; set; }
}