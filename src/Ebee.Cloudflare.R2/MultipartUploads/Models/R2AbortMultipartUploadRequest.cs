namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Request to abort a multipart upload.
/// </summary>
public class R2AbortMultipartUploadRequest
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
    /// Gets or sets the expected bucket owner.
    /// </summary>
    public string? ExpectedBucketOwner { get; set; }
}
