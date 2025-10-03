namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Request to list multipart uploads in a bucket.
/// </summary>
public class R2ListMultipartUploadsRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the prefix to filter uploads.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the delimiter for grouping keys.
    /// </summary>
    public string? Delimiter { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of uploads to return.
    /// </summary>
    public int MaxUploads { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the key marker to start listing from.
    /// </summary>
    public string? KeyMarker { get; set; }

    /// <summary>
    /// Gets or sets the upload ID marker to start listing from.
    /// </summary>
    public string? UploadIdMarker { get; set; }

    /// <summary>
    /// Gets or sets the expected bucket owner.
    /// </summary>
    public string? ExpectedBucketOwner { get; set; }
}
