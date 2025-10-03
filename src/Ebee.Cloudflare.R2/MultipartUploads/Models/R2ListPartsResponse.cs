namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Response from listing parts of a multipart upload.
/// </summary>
public class R2ListPartsResponse
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
    /// Gets or sets the list of parts.
    /// </summary>
    public List<R2PartInfoResponse> Parts { get; set; } = [];

    /// <summary>
    /// Gets or sets the maximum number of parts returned.
    /// </summary>
    public int MaxParts { get; set; }

    /// <summary>
    /// Gets or sets whether the result is truncated.
    /// </summary>
    public bool IsTruncated { get; set; }

    /// <summary>
    /// Gets or sets the next part number marker for pagination.
    /// </summary>
    public int? NextPartNumberMarker { get; set; }

    /// <summary>
    /// Gets or sets the storage class of the object.
    /// </summary>
    public string? StorageClass { get; set; }

    /// <summary>
    /// Gets or sets the owner of the multipart upload.
    /// </summary>
    public string? Owner { get; set; }
}
