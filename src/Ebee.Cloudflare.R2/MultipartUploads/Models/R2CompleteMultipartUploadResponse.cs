namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Response from completing a multipart upload.
/// </summary>
public class R2CompleteMultipartUploadResponse
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
    /// Gets or sets the ETag of the completed object.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the location URL of the completed object.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the completed object.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method used.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets when the multipart upload was completed.
    /// </summary>
    public DateTime CompletedAt { get; set; }
}
