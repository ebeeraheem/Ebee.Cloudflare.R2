namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Information about a multipart upload.
/// </summary>
public class R2MultipartUploadInfoResponse
{
    /// <summary>
    /// Gets or sets the object key.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the upload ID.
    /// </summary>
    public required string UploadId { get; set; }

    /// <summary>
    /// Gets or sets when the multipart upload was initiated.
    /// </summary>
    public DateTime Initiated { get; set; }

    /// <summary>
    /// Gets or sets the storage class of the object.
    /// </summary>
    public string? StorageClass { get; set; }

    /// <summary>
    /// Gets or sets the owner of the multipart upload.
    /// </summary>
    public string? Owner { get; set; }
}