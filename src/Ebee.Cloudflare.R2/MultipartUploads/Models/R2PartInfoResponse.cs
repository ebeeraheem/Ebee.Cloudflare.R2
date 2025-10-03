namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Information about a part in a multipart upload.
/// </summary>
public class R2PartInfoResponse
{
    /// <summary>
    /// Gets or sets the part number.
    /// </summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the ETag of the part.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the size of the part in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets when the part was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }
}
