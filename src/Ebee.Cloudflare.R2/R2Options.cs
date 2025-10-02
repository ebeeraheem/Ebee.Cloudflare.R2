namespace Ebee.Cloudflare.R2;

/// <summary>
/// Configuration options for Cloudflare R2 client.
/// </summary>
public class R2Options
{
    /// <summary>
    /// Gets or sets the Cloudflare account ID.
    /// </summary>
    public required string AccountId { get; set; }

    /// <summary>
    /// Gets or sets the R2 access key ID.
    /// </summary>
    public required string AccessKeyId { get; set; }

    /// <summary>
    /// Gets or sets the R2 secret access key.
    /// </summary>
    public required string SecretAccessKey { get; set; }

    /// <summary>
    /// Gets or sets the R2 endpoint URL. If not provided, will be constructed from AccountId.
    /// </summary>
    public string? EndpointUrl { get; set; }

    /// <summary>
    /// Gets the constructed endpoint URL for R2.
    /// </summary>
    public string GetEndpointUrl() => EndpointUrl ?? $"https://{AccountId}.r2.cloudflarestorage.com";
}