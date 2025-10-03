namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Response containing a list of R2 objects.
/// </summary>
public class R2ListObjectsResponse
{
    /// <summary>
    /// Gets or sets the list of objects.
    /// </summary>
    public List<R2ObjectInfoResponse> Objects { get; set; } = [];

    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the prefix used for filtering.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the delimiter used for grouping.
    /// </summary>
    public string? Delimiter { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of keys returned.
    /// </summary>
    public int MaxKeys { get; set; }

    /// <summary>
    /// Gets or sets whether the result is truncated.
    /// </summary>
    public bool IsTruncated { get; set; }

    /// <summary>
    /// Gets or sets the continuation token for next page.
    /// </summary>
    public string? NextContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets the key count.
    /// </summary>
    public int KeyCount { get; set; }

    /// <summary>
    /// Gets or sets the common prefixes for grouped results.
    /// </summary>
    public List<string> CommonPrefixes { get; set; } = [];
}