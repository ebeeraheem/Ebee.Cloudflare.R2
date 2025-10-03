using Ebee.Cloudflare.R2.Objects.Models;

namespace Ebee.Cloudflare.R2.Objects;

/// <summary>
/// Interface for R2 object operations.
/// </summary>
public interface IObjectsClient
{
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
    /// var response = await objectsClient.ListObjectsAsync(request);
    /// </code>
    /// </example>
    Task<R2ListObjectsResponse> ListObjectsAsync(
        R2ListObjectsRequest request,
        CancellationToken cancellationToken = default);

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
    /// var response = await objectsClient.GetObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2GetObjectResponse> GetObjectAsync(
        R2GetObjectRequest request,
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
    /// var response = await objectsClient.PutObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2PutObjectResponse> PutObjectAsync(
        R2PutObjectRequest request,
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
    /// var response = await objectsClient.DeleteObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2DeleteObjectResponse> DeleteObjectAsync(
        R2DeleteObjectRequest request,
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
    /// var response = await objectsClient.GetObjectMetadataAsync(request);
    /// </code>
    /// </example>
    Task<R2GetObjectMetadataResponse> GetObjectMetadataAsync(
        R2GetObjectMetadataRequest request,
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
    /// var response = await objectsClient.CopyObjectAsync(request);
    /// </code>
    /// </example>
    Task<R2CopyObjectResponse> CopyObjectAsync(
        R2CopyObjectRequest request,
        CancellationToken cancellationToken = default);
}
