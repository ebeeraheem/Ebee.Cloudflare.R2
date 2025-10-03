namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Response from uploading a part in a multipart upload.
/// </summary>
public class R2UploadPartResponse
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
    /// Gets or sets the upload ID.
    /// </summary>
    public required string UploadId { get; set; }

    /// <summary>
    /// Gets or sets the part number.
    /// </summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the ETag of the uploaded part.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption method used.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets when the part was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}
