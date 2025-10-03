using Ebee.Cloudflare.R2.SignedUrls.Models;

namespace Ebee.Cloudflare.R2.SignedUrls;

/// <summary>
/// Interface for R2 signed URL operations.
/// </summary>
public interface ISignedUrlsClient
{
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
    /// var response = signedUrlsClient.GenerateGetSignedUrl(request);
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
    /// var response = signedUrlsClient.GeneratePutSignedUrl(request);
    /// </code>
    /// </example>
    R2SignedUrlResponse GeneratePutSignedUrl(R2GeneratePutSignedUrlRequest request);

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
    /// var response = signedUrlsClient.GenerateDeleteSignedUrl(request);
    /// </code>
    /// </example>
    R2SignedUrlResponse GenerateDeleteSignedUrl(R2GenerateDeleteSignedUrlRequest request);
}