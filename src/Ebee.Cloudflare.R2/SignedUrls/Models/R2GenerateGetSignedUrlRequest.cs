namespace Ebee.Cloudflare.R2.SignedUrls.Models;

/// <summary>
/// Request to generate a signed URL for downloading an object from R2.
/// </summary>
public class R2GenerateGetSignedUrlRequest : R2GenerateSignedUrlRequest
{
    /// <summary>
    /// Gets or sets the response content type.
    /// </summary>
    public string? ResponseContentType { get; set; }

    /// <summary>
    /// Gets or sets the response content disposition.
    /// </summary>
    public string? ResponseContentDisposition { get; set; }

    /// <summary>
    /// Gets or sets the response cache control.
    /// </summary>
    public string? ResponseCacheControl { get; set; }

    /// <summary>
    /// Gets or sets the response expires header.
    /// </summary>
    public DateTime? ResponseExpires { get; set; }
}
