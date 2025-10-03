namespace Ebee.Cloudflare.R2.MultipartUploads.Models;

/// <summary>
/// Request to initiate a multipart upload for an R2 object.
/// </summary>
public class R2InitiateMultipartUploadRequest
{
    /// <summary>
    /// Gets or sets the name of the bucket to upload the object to.
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Gets or sets the key (name/path) of the object to upload.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the content type of the object.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the custom metadata for the object.
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
}
