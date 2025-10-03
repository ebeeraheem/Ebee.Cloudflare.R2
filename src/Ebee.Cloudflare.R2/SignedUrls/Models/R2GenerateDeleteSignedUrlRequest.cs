namespace Ebee.Cloudflare.R2.SignedUrls.Models;

/// <summary>
/// Request to generate a signed URL for deleting an object from R2.
/// </summary>
public class R2GenerateDeleteSignedUrlRequest : R2GenerateSignedUrlRequest
{
    /// <summary>
    /// Gets or sets whether to bypass governance mode restrictions.
    /// </summary>
    public bool BypassGovernanceRetention { get; set; }

    /// <summary>
    /// Gets or sets the expected bucket owner.
    /// </summary>
    public string? ExpectedBucketOwner { get; set; }
}