using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.MultipartUploads;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.SignedUrls;

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
}
