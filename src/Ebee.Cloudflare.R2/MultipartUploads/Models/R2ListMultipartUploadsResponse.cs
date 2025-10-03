namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Response from listing multipart uploads in a bucket.
/// </summary>
public class R2ListMultipartUploadsResponse
{
    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the list of multipart uploads.
    /// </summary>
    public List<R2MultipartUploadInfoResponse> Uploads { get; set; } = [];

    /// <summary>
    /// Gets or sets the prefix used for filtering.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the delimiter used for grouping.
    /// </summary>
    public string? Delimiter { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of uploads returned.
    /// </summary>
    public int MaxUploads { get; set; }

    /// <summary>
    /// Gets or sets whether the result is truncated.
    /// </summary>
    public bool IsTruncated { get; set; }

    /// <summary>
    /// Gets or sets the next key marker for pagination.
    /// </summary>
    public string? NextKeyMarker { get; set; }

    /// <summary>
    /// Gets or sets the next upload ID marker for pagination.
    /// </summary>
    public string? NextUploadIdMarker { get; set; }

    /// <summary>
    /// Gets or sets the common prefixes for grouped results.
    /// </summary>
    public List<string> CommonPrefixes { get; set; } = [];
}
