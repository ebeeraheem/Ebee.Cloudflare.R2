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
/// Main client for Cloudflare R2 operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="R2Client"/> class.
/// </remarks>
/// <param name="bucketsClient">The buckets client.</param>
/// <param name="objectsClient">The objects client.</param>
/// <param name="signedUrlsClient">The signed URLs client.</param>
/// <param name="multipartUploadsClient">The multipart uploads client.</param>
public class R2Client(
    IBucketsClient bucketsClient,
    IObjectsClient objectsClient,
    ISignedUrlsClient signedUrlsClient,
    IMultipartUploadsClient multipartUploadsClient) : IR2Client
{
    /// <inheritdoc />
    public IBucketsClient Buckets { get; } = bucketsClient
        ?? throw new ArgumentNullException(nameof(bucketsClient));

    /// <inheritdoc />
    public IObjectsClient Objects { get; } = objectsClient
        ?? throw new ArgumentNullException(nameof(objectsClient));

    /// <inheritdoc />
    public ISignedUrlsClient SignedUrls { get; } = signedUrlsClient
        ?? throw new ArgumentNullException(nameof(signedUrlsClient));

    /// <inheritdoc />
    public IMultipartUploadsClient MultipartUploads { get; } = multipartUploadsClient
        ?? throw new ArgumentNullException(nameof(multipartUploadsClient));

    // Direct bucket operations
    /// <inheritdoc />
    public Task<R2ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default)
        => Buckets.ListBucketsAsync(cancellationToken);

    /// <inheritdoc />
    public Task<R2CreateBucketResponse> CreateBucketAsync(
        R2CreateBucketRequest request,
        CancellationToken cancellationToken = default)
        => Buckets.CreateBucketAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2DeleteBucketResponse> DeleteBucketAsync(
        R2DeleteBucketRequest request,
        CancellationToken cancellationToken = default)
        => Buckets.DeleteBucketAsync(request, cancellationToken);

    // Direct object operations
    /// <inheritdoc />
    public Task<R2ListObjectsResponse> ListObjectsAsync(
        R2ListObjectsRequest request,
        CancellationToken cancellationToken = default)
        => Objects.ListObjectsAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2GetObjectResponse> GetObjectAsync(
        R2GetObjectRequest request,
        CancellationToken cancellationToken = default)
        => Objects.GetObjectAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2PutObjectResponse> PutObjectAsync(
        R2PutObjectRequest request,
        CancellationToken cancellationToken = default)
        => Objects.PutObjectAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2DeleteObjectResponse> DeleteObjectAsync(
        R2DeleteObjectRequest request,
        CancellationToken cancellationToken = default)
        => Objects.DeleteObjectAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2GetObjectMetadataResponse> GetObjectMetadataAsync(
        R2GetObjectMetadataRequest request,
        CancellationToken cancellationToken = default)
        => Objects.GetObjectMetadataAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2CopyObjectResponse> CopyObjectAsync(
        R2CopyObjectRequest request,
        CancellationToken cancellationToken = default)
        => Objects.CopyObjectAsync(request, cancellationToken);

    // Direct signed URL operations
    /// <inheritdoc />
    public R2SignedUrlResponse GenerateGetSignedUrl(R2GenerateGetSignedUrlRequest request)
        => SignedUrls.GenerateGetSignedUrl(request);

    /// <inheritdoc />
    public R2SignedUrlResponse GeneratePutSignedUrl(R2GeneratePutSignedUrlRequest request)
        => SignedUrls.GeneratePutSignedUrl(request);

    /// <inheritdoc />
    public R2SignedUrlResponse GenerateDeleteSignedUrl(R2GenerateDeleteSignedUrlRequest request)
        => SignedUrls.GenerateDeleteSignedUrl(request);

    // Direct multipart upload operations
    /// <inheritdoc />
    public Task<R2InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(
        R2InitiateMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
        => MultipartUploads.InitiateMultipartUploadAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2UploadPartResponse> UploadPartAsync(
        R2UploadPartRequest request,
        CancellationToken cancellationToken = default)
        => MultipartUploads.UploadPartAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(
        R2CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
        => MultipartUploads.CompleteMultipartUploadAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2AbortMultipartUploadResponse> AbortMultipartUploadAsync(
        R2AbortMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
        => MultipartUploads.AbortMultipartUploadAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2ListPartsResponse> ListPartsAsync(
        R2ListPartsRequest request,
        CancellationToken cancellationToken = default)
        => MultipartUploads.ListPartsAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<R2ListMultipartUploadsResponse> ListMultipartUploadsAsync(
        R2ListMultipartUploadsRequest request,
        CancellationToken cancellationToken = default)
        => MultipartUploads.ListMultipartUploadsAsync(request, cancellationToken);
}
