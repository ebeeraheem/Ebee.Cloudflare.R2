using Ebee.Cloudflare.R2.MultipartUploads.Models;

namespace Ebee.Cloudflare.R2.MultipartUploads;

/// <summary>
/// Interface for R2 multipart upload operations.
/// </summary>
public interface IMultipartUploadsClient
{
    /// <summary>
    /// Initiates a multipart upload for an R2 object.
    /// </summary>
    /// <param name="request">The initiate multipart upload request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the initiate response.</returns>
    /// <exception cref="R2Exception">Thrown when the initiation fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2InitiateMultipartUploadRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "large-file.zip",
    ///     ContentType = "application/zip"
    /// };
    /// var response = await multipartClient.InitiateMultipartUploadAsync(request);
    /// </code>
    /// </example>
    Task<R2InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(
        R2InitiateMultipartUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a part in a multipart upload.
    /// </summary>
    /// <param name="request">The upload part request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the upload part response.</returns>
    /// <exception cref="R2Exception">Thrown when the part upload fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2UploadPartRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "large-file.zip",
    ///     UploadId = "upload-id-from-initiate",
    ///     PartNumber = 1,
    ///     ContentBytes = partData
    /// };
    /// var response = await multipartClient.UploadPartAsync(request);
    /// </code>
    /// </example>
    Task<R2UploadPartResponse> UploadPartAsync(
        R2UploadPartRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a multipart upload by assembling the uploaded parts.
    /// </summary>
    /// <param name="request">The complete multipart upload request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the complete response.</returns>
    /// <exception cref="R2Exception">Thrown when the completion fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2CompleteMultipartUploadRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "large-file.zip",
    ///     UploadId = "upload-id-from-initiate",
    ///     Parts = new List&lt;R2CompletedPart&gt;
    ///     {
    ///         new() { PartNumber = 1, ETag = "etag-from-part-1" },
    ///         new() { PartNumber = 2, ETag = "etag-from-part-2" }
    ///     }
    /// };
    /// var response = await multipartClient.CompleteMultipartUploadAsync(request);
    /// </code>
    /// </example>
    Task<R2CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(
        R2CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a multipart upload and removes any uploaded parts.
    /// </summary>
    /// <param name="request">The abort multipart upload request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the abort response.</returns>
    /// <exception cref="R2Exception">Thrown when the abort fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2AbortMultipartUploadRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "large-file.zip",
    ///     UploadId = "upload-id-from-initiate"
    /// };
    /// var response = await multipartClient.AbortMultipartUploadAsync(request);
    /// </code>
    /// </example>
    Task<R2AbortMultipartUploadResponse> AbortMultipartUploadAsync(
        R2AbortMultipartUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the parts of a multipart upload.
    /// </summary>
    /// <param name="request">The list parts request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the list of parts.</returns>
    /// <exception cref="R2Exception">Thrown when the listing fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2ListPartsRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "large-file.zip",
    ///     UploadId = "upload-id-from-initiate"
    /// };
    /// var response = await multipartClient.ListPartsAsync(request);
    /// </code>
    /// </example>
    Task<R2ListPartsResponse> ListPartsAsync(
        R2ListPartsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all ongoing multipart uploads in a bucket.
    /// </summary>
    /// <param name="request">The list multipart uploads request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the list of uploads.</returns>
    /// <exception cref="R2Exception">Thrown when the listing fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2ListMultipartUploadsRequest { BucketName = "my-bucket" };
    /// var response = await multipartClient.ListMultipartUploadsAsync(request);
    /// </code>
    /// </example>
    Task<R2ListMultipartUploadsResponse> ListMultipartUploadsAsync(
        R2ListMultipartUploadsRequest request,
        CancellationToken cancellationToken = default);
}
