namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Represents a completed part in a multipart upload.
/// </summary>
public class R2CompletedPart
{
    /// <summary>
    /// Gets or sets the part number.
    /// </summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the ETag of the part.
    /// </summary>
    public required string ETag { get; set; }
}
