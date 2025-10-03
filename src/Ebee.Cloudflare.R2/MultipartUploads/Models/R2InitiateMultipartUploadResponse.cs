namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Response from initiating a multipart upload for an R2 object.
/// </summary>
public class R2InitiateMultipartUploadResponse
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
    /// Gets or sets the upload ID for the multipart upload.
    /// </summary>
    public required string UploadId { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method used.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets when the multipart upload was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; set; }
}
