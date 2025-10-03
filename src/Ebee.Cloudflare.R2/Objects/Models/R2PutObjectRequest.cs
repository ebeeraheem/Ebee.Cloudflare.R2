namespace Ebee.Cloudflare.R2.Objects.Models;

/// <summary>
/// Request to put an object into an R2 bucket.
/// </summary>
public class R2PutObjectRequest : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Gets or sets the name of the bucket to store the object in.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the key (name/path) of the object to store.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the content stream to upload.
    /// </summary>
    public Stream? ContentStream { get; set; }

    /// <summary>
    /// Gets or sets the content as byte array to upload.
    /// </summary>
    public byte[]? ContentBytes { get; set; }

    /// <summary>
    /// Gets or sets the file path to upload from.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Gets or sets the server-side encryption method.
    /// </summary>
    public string? ServerSideEncryption { get; set; }

    /// <summary>
    /// Gets or sets the storage class.
    /// </summary>
    public string? StorageClass { get; set; }

    /// <summary>
    /// Gets or sets whether to use server-side encryption with customer-provided keys.
    /// </summary>
    public string? SSECustomerAlgorithm { get; set; }

    /// <summary>
    /// Gets or sets the customer-provided encryption key.
    /// </summary>
    public string? SSECustomerKey { get; set; }

    /// <summary>
    /// Gets or sets the MD5 hash of the customer-provided encryption key.
    /// </summary>
    public string? SSECustomerKeyMD5 { get; set; }

    /// <summary>
    /// Finalizer for R2PutObjectRequest.
    /// </summary>
    ~R2PutObjectRequest()
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
