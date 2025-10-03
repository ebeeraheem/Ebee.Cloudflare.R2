namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Request to list objects in an R2 bucket.
/// </summary>
public class R2ListObjectsRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket to list objects from.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the prefix to filter objects.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the delimiter for grouping keys.
    /// </summary>
    public string? Delimiter { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of objects to return.
    /// </summary>
    public int MaxKeys { get; set; } = 100;

    /// <summary>
    /// Gets or sets the continuation token for pagination.
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets whether to fetch owner information.
    /// </summary>
    public bool FetchOwner { get; set; }

    /// <summary>
    /// Gets or sets the start after key for pagination.
    /// </summary>
    public string? StartAfter { get; set; }
}