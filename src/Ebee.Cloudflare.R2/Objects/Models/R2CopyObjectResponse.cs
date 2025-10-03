namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Response from copying an object within or between R2 buckets.
/// </summary>
public class R2CopyObjectResponse
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
    /// Gets or sets the ETag of the copied object.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the copied object.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the copied object.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets the copy timestamp.
    /// </summary>
    public DateTime CopiedAt { get; set; }
}