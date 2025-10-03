namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Request to upload a part in a multipart upload.
/// </summary>
public class R2UploadPartRequest : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Gets or sets the name of the bucket.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the object key.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the upload ID from the initiate multipart upload response.
    /// </summary>
    public required string UploadId { get; set; }

    /// <summary>
    /// Gets or sets the part number (1-10000).
    /// </summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the content stream for the part.
    /// </summary>
    public Stream? ContentStream { get; set; }

    /// <summary>
    /// Gets or sets the content as byte array for the part.
    /// </summary>
    public byte[]? ContentBytes { get; set; }

    /// <summary>
    /// Gets or sets the file path to upload the part from.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the expected MD5 hash of the part content.
    /// </summary>
    public string? ContentMD5 { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption customer algorithm.
    /// </summary>
    public string? SSECustomerAlgorithm { get; set; }

    /// <summary>
    /// Gets or sets the server-side encryption customer key.
    /// </summary>
    public string? SSECustomerKey { get; set; }

    /// <summary>
    /// Gets or sets the MD5 hash of the server-side encryption customer key.
    /// </summary>
    public string? SSECustomerKeyMD5 { get; set; }

    /// <summary>
    /// Finalizer for R2UploadPartRequest.
    /// </summary>
    ~R2UploadPartRequest()
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
        if (!_disposed && disposing)
        {
            ContentStream?.Dispose();
        }
        _disposed = true;
    }
}
