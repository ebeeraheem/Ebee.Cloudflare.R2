namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Represents information about an R2 object.
/// </summary>
public class R2ObjectInfoResponse
{
    /// <summary>
    /// Gets or sets the object key (name/path).
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the size of the object in bytes.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the object.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets the ETag of the object.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the storage class of the object.
    /// </summary>
    public string? StorageClass { get; set; }

    /// <summary>
    /// Gets or sets the owner information.
    /// </summary>
    public string? Owner { get; set; }
}
