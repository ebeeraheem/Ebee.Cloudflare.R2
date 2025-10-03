namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Request to delete an object from an R2 bucket.
/// </summary>
public class R2DeleteObjectRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket containing the object to delete.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the key (name/path) of the object to delete.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the object to delete.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets whether to bypass governance mode restrictions.
    /// </summary>
    public bool BypassGovernanceRetention { get; set; }

    /// <summary>
    /// Gets or sets the expected bucket owner.
    /// </summary>
    public string? ExpectedBucketOwner { get; set; }
}