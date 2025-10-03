namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Response from aborting a multipart upload.
/// </summary>
public class R2AbortMultipartUploadResponse
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
    /// Gets or sets the upload ID that was aborted.
    /// </summary>
    public required string UploadId { get; set; }

    /// <summary>
    /// Gets or sets when the multipart upload was aborted.
    /// </summary>
    public DateTime AbortedAt { get; set; }
}
