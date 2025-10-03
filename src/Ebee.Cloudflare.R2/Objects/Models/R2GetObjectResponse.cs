namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Response from getting an object from an R2 bucket.
/// </summary>
public class R2GetObjectResponse : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Gets or sets the bucket name.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the object key.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the content stream.
    /// </summary>
    public Stream? ContentStream { get; set; }

    /// <summary>
    /// Gets or sets the content as byte array.
    /// </summary>
    public byte[]? ContentBytes { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content length.
    /// </summary>
    public long ContentLength { get; set; }

    /// <summary>
    /// Gets or sets the ETag.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets the version ID.
    /// </summary>
    public string? VersionId { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets the cache control header.
    /// </summary>
    public string? CacheControl { get; set; }

    /// <summary>
    /// Gets or sets the content disposition header.
    /// </summary>
    public string? ContentDisposition { get; set; }

    /// <summary>
    /// Gets or sets the content encoding header.
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets the expires header.
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Finalizer for R2GetObjectResponse.
    /// </summary>
    ~R2GetObjectResponse()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes the content stream if it exists.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of dispose pattern.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                ContentStream?.Dispose();
            }
            _disposed = true;
        }
    }
}
