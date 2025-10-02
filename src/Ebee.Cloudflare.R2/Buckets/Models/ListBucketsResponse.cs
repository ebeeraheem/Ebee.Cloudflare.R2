namespace Ebee.Cloudflare.R2.Buckets.Models;

/// <summary>
/// Response containing a list of R2 buckets.
/// </summary>
public class ListBucketsResponse
{
    /// <summary>
    /// Gets or sets the list of buckets.
    /// </summary>
    public List<BucketInfoResponse> Buckets { get; set; } = [];

    /// <summary>
    /// Gets or sets the owner information.
    /// </summary>
    public string? Owner { get; set; }
}
