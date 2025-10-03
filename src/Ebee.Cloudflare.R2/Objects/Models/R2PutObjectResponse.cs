namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Response from putting an object into an R2 bucket.
/// </summary>
public class R2PutObjectResponse
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
    /// Gets or sets the ETag of the uploaded object.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the uploaded object.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method used.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets the upload timestamp.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}