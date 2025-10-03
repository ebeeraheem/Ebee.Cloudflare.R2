namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Request to complete a multipart upload.
/// </summary>
public class R2CompleteMultipartUploadRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the object key.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the upload ID from the initiate multipart upload response.
    /// </summary>
    public required string UploadId { get; set; }

    /// <summary>
    /// Gets or sets the list of parts to complete the upload.
    /// </summary>
    public List<R2CompletedPart> Parts { get; set; } = [];
}
