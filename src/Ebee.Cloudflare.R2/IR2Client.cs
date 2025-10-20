using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Buckets.Models;
using Ebee.Cloudflare.R2.MultipartUploads;
using Ebee.Cloudflare.R2.MultipartUploads.Models;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.Objects.Models;
using Ebee.Cloudflare.R2.SignedUrls;
using Ebee.Cloudflare.R2.SignedUrls.Models;

namespace Ebee.Cloudflare.R2;

/// <summary>
/// Interface for the main R2 client.
/// </summary>
public interface IR2Client
{
    /// <summary>
    /// Gets the buckets client for bucket operations.
    /// </summary>
    IBucketsClient Buckets { get; }

    /// <summary>
    /// Gets the objects client for object operations.
    /// </summary>
    IObjectsClient Objects { get; }

    /// <summary>
    /// Gets the signed URLs client for signed URL operations.
    /// </summary>
    ISignedUrlsClient SignedUrls { get; }

    /// <summary>
    /// Gets the multipart uploads client for multipart upload operations.
    /// </summary>
    IMultipartUploadsClient MultipartUploads { get; }

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
    /// var response = await r2Client.AbortMultipartUploadAsync(request);
    /// </code>
    /// </example>
    Task<R2AbortMultipartUploadResponse> AbortMultipartUploadAsync(
        R2AbortMultipartUploadRequest request,
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
    /// var response = await r2Client.CompleteMultipartUploadAsync(request);
    /// </code>
    /// </example>
    Task<R2CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(
        R2CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies an object within or between R2 buckets.
    /// </summary>
    /// <param name="request">The copy object request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the copy object response.</returns>
    /// <exception cref="R2Exception">Thrown when the object copy fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2CopyObjectRequest 
    /// { 
    ///     SourceBucketName = "source-bucket", 
    ///     SourceKey = "source-file.txt",
    ///     DestinationBucketName = "dest-bucket",
    ///     DestinationKey = "dest-file.txt"
    /// };
    /// var response = await r2Client.CopyObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2CopyObjectResponse> CopyObjectAsync(
        R2CopyObjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new bucket in the R2 account.
    /// </summary>
    /// <param name="request">The create bucket request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the create bucket response.</returns>
    /// <exception cref="R2Exception">Thrown when the bucket creation fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2CreateBucketRequest { BucketName = "my-bucket" };
    /// var response = await r2Client.CreateBucketAsync(request);
    /// </code>
    /// </example>
    Task<R2CreateBucketResponse> CreateBucketAsync(
        R2CreateBucketRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bucket from the R2 account.
    /// </summary>
    /// <param name="request">The delete bucket request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the delete bucket response.</returns>
    /// <exception cref="R2Exception">Thrown when the bucket deletion fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2DeleteBucketRequest { BucketName = "my-bucket" };
    /// var response = await r2Client.DeleteBucketAsync(request);
    /// </code>
    /// </example>
    Task<R2DeleteBucketResponse> DeleteBucketAsync(
        R2DeleteBucketRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an object from an R2 bucket.
    /// </summary>
    /// <param name="request">The delete object request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the delete object response.</returns>
    /// <exception cref="R2Exception">Thrown when the object deletion fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2DeleteObjectRequest { BucketName = "my-bucket", Key = "my-file.txt" };
    /// var response = await r2Client.DeleteObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2DeleteObjectResponse> DeleteObjectAsync(
        R2DeleteObjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a signed URL for deleting an object from an R2 bucket.
    /// </summary>
    /// <param name="request">The generate signed URL request.</param>
    /// <returns>The signed URL response.</returns>
    /// <exception cref="R2Exception">Thrown when the signed URL generation fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2GenerateDeleteSignedUrlRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "my-file.txt",
    ///     ExpiresIn = TimeSpan.FromMinutes(30)
    /// };
    /// var response = r2Client.GenerateDeleteSignedUrl(request);
    /// </code>
    /// </example>
    R2SignedUrlResponse GenerateDeleteSignedUrl(R2GenerateDeleteSignedUrlRequest request);

    /// <summary>
    /// Generates a signed URL for downloading an object from an R2 bucket.
    /// </summary>
    /// <param name="request">The generate signed URL request.</param>
    /// <returns>The signed URL response.</returns>
    /// <exception cref="R2Exception">Thrown when the signed URL generation fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2GenerateGetSignedUrlRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "my-file.txt",
    ///     ExpiresIn = TimeSpan.FromHours(1)
    /// };
    /// var response = r2Client.GenerateGetSignedUrl(request);
    /// </code>
    /// </example>
    R2SignedUrlResponse GenerateGetSignedUrl(R2GenerateGetSignedUrlRequest request);

    /// <summary>
    /// Generates a signed URL for uploading an object to an R2 bucket.
    /// </summary>
    /// <param name="request">The generate signed URL request.</param>
    /// <returns>The signed URL response.</returns>
    /// <exception cref="R2Exception">Thrown when the signed URL generation fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2GeneratePutSignedUrlRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "my-file.txt",
    ///     ContentType = "text/plain",
    ///     ExpiresIn = TimeSpan.FromHours(2)
    /// };
    /// var response = r2Client.GeneratePutSignedUrl(request);
    /// </code>
    /// </example>
    R2SignedUrlResponse GeneratePutSignedUrl(R2GeneratePutSignedUrlRequest request);

    /// <summary>
    /// Gets an object from an R2 bucket.
    /// </summary>
    /// <param name="request">The get object request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the object data.</returns>
    /// <exception cref="R2Exception">Thrown when the object retrieval fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2GetObjectRequest { BucketName = "my-bucket", Key = "my-file.txt" };
    /// var response = await r2Client.GetObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2GetObjectResponse> GetObjectAsync(
        R2GetObjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for an object in an R2 bucket without downloading the content.
    /// </summary>
    /// <param name="request">The get object metadata request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the object metadata.</returns>
    /// <exception cref="R2Exception">Thrown when the metadata retrieval fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2GetObjectMetadataRequest { BucketName = "my-bucket", Key = "my-file.txt" };
    /// var response = await r2Client.GetObjectMetadataAsync(request);
    /// </code>
    /// </example>
    Task<R2GetObjectMetadataResponse> GetObjectMetadataAsync(
        R2GetObjectMetadataRequest request,
        CancellationToken cancellationToken = default);

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
    /// var response = await r2Client.InitiateMultipartUploadAsync(request);
    /// </code>
    /// </example>
    Task<R2InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(
        R2InitiateMultipartUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all buckets in the R2 account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the list of buckets.</returns>
    /// <example>
    /// <code>
    /// var response = await r2Client.ListBucketsAsync();
    /// </code>
    /// </example>
    Task<R2ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default);

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
    /// var response = await r2Client.ListMultipartUploadsAsync(request);
    /// </code>
    /// </example>
    Task<R2ListMultipartUploadsResponse> ListMultipartUploadsAsync(
        R2ListMultipartUploadsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists objects in an R2 bucket.
    /// </summary>
    /// <param name="request">The list objects request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the list of objects.</returns>
    /// <exception cref="R2Exception">Thrown when the object listing fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2ListObjectsRequest { BucketName = "my-bucket" };
    /// var response = await r2Client.ListObjectsAsync(request);
    /// </code>
    /// </example>
    Task<R2ListObjectsResponse> ListObjectsAsync(
        R2ListObjectsRequest request,
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
    /// var response = await r2Client.ListPartsAsync(request);
    /// </code>
    /// </example>
    Task<R2ListPartsResponse> ListPartsAsync(
        R2ListPartsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts an object into an R2 bucket.
    /// </summary>
    /// <param name="request">The put object request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the put object response.</returns>
    /// <exception cref="R2Exception">Thrown when the object upload fails.</exception>
    /// <example>
    /// <code>
    /// var request = new R2PutObjectRequest 
    /// { 
    ///     BucketName = "my-bucket", 
    ///     Key = "my-file.txt",
    ///     ContentBytes = Encoding.UTF8.GetBytes("Hello World")
    /// };
    /// var response = await r2Client.PutObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2PutObjectResponse> PutObjectAsync(
        R2PutObjectRequest request,
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
    /// var response = await r2Client.UploadPartAsync(request);
    /// </code>
    /// </example>
    Task<R2UploadPartResponse> UploadPartAsync(
        R2UploadPartRequest request,
        CancellationToken cancellationToken = default);
}
