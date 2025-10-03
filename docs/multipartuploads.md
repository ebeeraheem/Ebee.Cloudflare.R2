# Multipart Uploads

The `MultipartUploadsClient` provides methods for managing R2 multipart uploads, enabling efficient uploading of large objects by breaking them into smaller parts that can be uploaded independently and in parallel.

## Getting Started

The `MultipartUploadsClient` is automatically registered when you configure the R2 client in your dependency injection container:

```csharp
services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key";
    options.SecretAccessKey = "your-secret-key";
    options.AccountId = "your-account-id";
});
```

Then inject `IR2Client` and access the multipart uploads client:

```csharp
public class MyService
{
    private readonly IR2Client _r2Client;

    public MyService(IR2Client r2Client)
    {
        _r2Client = r2Client;
    }

    public async Task ManageMultipartUploads()
    {
        var multipartClient = _r2Client.MultipartUploads;
        // Use multipart uploads client methods...
    }
}
```

## When to Use Multipart Uploads

Multipart uploads are recommended for:

- **Large Files**: Objects larger than 100MB benefit from multipart uploads
- **Unreliable Networks**: Failed parts can be retried without affecting other parts
- **Parallel Uploads**: Multiple parts can be uploaded simultaneously for faster transfers
- **Resume Capability**: Interrupted uploads can be resumed from where they left off

## Basic Multipart Upload Flow

A complete multipart upload consists of three steps:

1. **Initiate** the multipart upload to get an upload ID
2. **Upload** one or more parts (1-10,000 parts allowed)
3. **Complete** the upload by providing the list of uploaded parts

## Initiating Multipart Uploads

Start a multipart upload to get an upload ID that will be used for all subsequent operations:

```csharp
var request = new R2InitiateMultipartUploadRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    ContentType = "application/zip",
    Metadata = new Dictionary<string, string>
    {
        { "uploader", "multipart-service" },
        { "file-category", "backup" }
    }
};

var response = await _r2Client.MultipartUploads.InitiateMultipartUploadAsync(request);

Console.WriteLine($"Initiated upload with ID: {response.UploadId}");
Console.WriteLine($"Upload started at: {response.InitiatedAt}");

// Store the upload ID for subsequent operations
string uploadId = response.UploadId;
```

### Advanced Initiation Options

```csharp
var request = new R2InitiateMultipartUploadRequest
{
    BucketName = "my-bucket",
    Key = "sensitive-data.zip",
    ContentType = "application/zip",
    ServerSideEncryption = "AES256",
    StorageClass = "STANDARD",
    CacheControl = "max-age=3600",
    ContentDisposition = "attachment; filename=\"backup.zip\"",
    ContentEncoding = "gzip",
    Expires = DateTime.UtcNow.AddDays(30),
    
    // Server-side encryption with customer-provided keys
    SSECustomerAlgorithm = "AES256",
    SSECustomerKey = "your-base64-encoded-256-bit-key",
    SSECustomerKeyMD5 = "md5-hash-of-the-key",
    
    Metadata = new Dictionary<string, string>
    {
        { "project", "data-backup" },
        { "department", "engineering" }
    }
};

var response = await _r2Client.MultipartUploads.InitiateMultipartUploadAsync(request);
```

## Uploading Parts

Upload parts using the upload ID from the initiation step. Each part must be at least 5MB (except the last part) and parts are numbered from 1 to 10,000:

### Upload Part from Byte Array

```csharp
var partData = await File.ReadAllBytesAsync("part1.bin");

var request = new R2UploadPartRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId,
    PartNumber = 1,
    ContentBytes = partData,
    ContentMD5 = "md5-hash-of-part-content"  // Optional but recommended
};

var response = await _r2Client.MultipartUploads.UploadPartAsync(request);

Console.WriteLine($"Uploaded part {response.PartNumber} with ETag: {response.ETag}");

// Store the ETag for completion
string partETag = response.ETag;
```

### Upload Part from Stream

```csharp
using var partStream = File.OpenRead("part2.bin");

var request = new R2UploadPartRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId,
    PartNumber = 2,
    ContentStream = partStream
};

var response = await _r2Client.MultipartUploads.UploadPartAsync(request);
```

### Upload Part from File Path

```csharp
var request = new R2UploadPartRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId,
    PartNumber = 3,
    FilePath = @"C:\temp\part3.bin"
};

var response = await _r2Client.MultipartUploads.UploadPartAsync(request);
```

### Upload Part with Customer Encryption

```csharp
var request = new R2UploadPartRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId,
    PartNumber = 4,
    ContentBytes = partData,
    SSECustomerAlgorithm = "AES256",
    SSECustomerKey = "your-base64-encoded-256-bit-key",
    SSECustomerKeyMD5 = "md5-hash-of-the-key"
};

var response = await _r2Client.MultipartUploads.UploadPartAsync(request);
```

### Parallel Part Uploads

```csharp
var uploadTasks = new List<Task<R2UploadPartResponse>>();

for (int partNumber = 1; partNumber <= totalParts; partNumber++)
{
    var partData = GetPartData(partNumber); // Your method to get part data
    
    var request = new R2UploadPartRequest
    {
        BucketName = "my-bucket",
        Key = "large-file.zip",
        UploadId = uploadId,
        PartNumber = partNumber,
        ContentBytes = partData
    };
    
    uploadTasks.Add(_r2Client.MultipartUploads.UploadPartAsync(request));
}

var responses = await Task.WhenAll(uploadTasks);

// Collect ETags for completion
var completedParts = responses
    .OrderBy(r => r.PartNumber)
    .Select(r => new R2CompletedPart 
    { 
        PartNumber = r.PartNumber, 
        ETag = r.ETag 
    })
    .ToList();
```

## Completing Multipart Uploads

Once all parts are uploaded, complete the multipart upload by providing the list of parts with their ETags:

```csharp
var completedParts = new List<R2CompletedPart>
{
    new() { PartNumber = 1, ETag = "\"etag-from-part-1\"" },
    new() { PartNumber = 2, ETag = "\"etag-from-part-2\"" },
    new() { PartNumber = 3, ETag = "\"etag-from-part-3\"" }
};

var request = new R2CompleteMultipartUploadRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId,
    Parts = completedParts
};

var response = await _r2Client.MultipartUploads.CompleteMultipartUploadAsync(request);

Console.WriteLine($"Upload completed successfully!");
Console.WriteLine($"Final object ETag: {response.ETag}");
Console.WriteLine($"Object location: {response.Location}");
Console.WriteLine($"Completed at: {response.CompletedAt}");
```

### Response Details

The `CompleteMultipartUploadAsync` method returns an `R2CompleteMultipartUploadResponse` containing:

- `ETag`: The ETag of the final assembled object
- `Location`: The URL of the completed object
- `VersionId`: Version ID if bucket versioning is enabled
- `ServerSideEncryption`: Encryption method used
- `CompletedAt`: Timestamp when the upload was completed

## Aborting Multipart Uploads

If an upload fails or is no longer needed, abort it to clean up the uploaded parts and avoid storage charges:

```csharp
var request = new R2AbortMultipartUploadRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId
};

var response = await _r2Client.MultipartUploads.AbortMultipartUploadAsync(request);

Console.WriteLine($"Aborted upload {response.UploadId} for {response.Key}");
Console.WriteLine($"Aborted at: {response.AbortedAt}");
```

### Important Notes

- Aborting an upload removes all uploaded parts
- Storage charges stop accruing for the aborted upload
- The upload ID becomes invalid after aborting
- This operation cannot be undone

## Listing Parts

List the parts that have been uploaded for a specific multipart upload:

```csharp
var request = new R2ListPartsRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    UploadId = uploadId,
    MaxParts = 100,                    // Optional: limit results
    PartNumberMarker = 50              // Optional: start after part number
};

var response = await _r2Client.MultipartUploads.ListPartsAsync(request);

Console.WriteLine($"Upload has {response.Parts.Count} parts uploaded");
Console.WriteLine($"Storage class: {response.StorageClass}");
Console.WriteLine($"Owner: {response.Owner}");

foreach (var part in response.Parts)
{
    Console.WriteLine($"Part {part.PartNumber}: {part.Size} bytes, " +
                     $"ETag: {part.ETag}, Modified: {part.LastModified}");
}

// Handle pagination
if (response.IsTruncated)
{
    var nextRequest = request with { PartNumberMarker = response.NextPartNumberMarker };
    var nextPage = await _r2Client.MultipartUploads.ListPartsAsync(nextRequest);
}
```

## Listing Multipart Uploads

List all ongoing multipart uploads in a bucket:

```csharp
var request = new R2ListMultipartUploadsRequest
{
    BucketName = "my-bucket",
    Prefix = "backup/",                // Optional: filter by prefix
    Delimiter = "/",                   // Optional: group by delimiter
    MaxUploads = 100,                  // Optional: limit results
    KeyMarker = "last-key-from-previous-page",      // Optional: pagination
    UploadIdMarker = "last-upload-id-from-previous-page" // Optional: pagination
};

var response = await _r2Client.MultipartUploads.ListMultipartUploadsAsync(request);

Console.WriteLine($"Found {response.Uploads.Count} ongoing uploads in bucket {response.BucketName}");

foreach (var upload in response.Uploads)
{
    Console.WriteLine($"Key: {upload.Key}");
    Console.WriteLine($"Upload ID: {upload.UploadId}");
    Console.WriteLine($"Initiated: {upload.Initiated}");
    Console.WriteLine($"Storage Class: {upload.StorageClass}");
    Console.WriteLine($"Owner: {upload.Owner}");
    Console.WriteLine("---");
}

// Handle pagination
if (response.IsTruncated)
{
    var nextRequest = request with 
    { 
        KeyMarker = response.NextKeyMarker, 
        UploadIdMarker = response.NextUploadIdMarker 
    };
    var nextPage = await _r2Client.MultipartUploads.ListMultipartUploadsAsync(nextRequest);
}
```

## Error Handling

All multipart upload operations may throw `R2Exception` with specific error scenarios:

```csharp
try
{
    var response = await _r2Client.MultipartUploads.UploadPartAsync(request);
    // Process successful response
}
catch (R2Exception ex)
{
    // Handle specific R2 errors
    Console.WriteLine($"Multipart upload operation failed: {ex.Message}");
    
    // Check inner exception for AWS S3 specific details
    if (ex.InnerException is AmazonS3Exception s3Ex)
    {
        Console.WriteLine($"S3 Error Code: {s3Ex.ErrorCode}");
    }
}
```

Common error scenarios:
- **NoSuchBucket**: The specified bucket doesn't exist
- **NoSuchUpload**: The specified upload ID doesn't exist
- **InvalidPart**: One or more parts are invalid (wrong ETag, missing part, etc.)
- **InvalidPartOrder**: Parts must be uploaded with consecutive part numbers

## API Reference

### IMultipartUploadsClient Interface

```csharp
public interface IMultipartUploadsClient
{
    Task<R2InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(
        R2InitiateMultipartUploadRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<R2UploadPartResponse> UploadPartAsync(
        R2UploadPartRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<R2CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(
        R2CompleteMultipartUploadRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<R2AbortMultipartUploadResponse> AbortMultipartUploadAsync(
        R2AbortMultipartUploadRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<R2ListPartsResponse> ListPartsAsync(
        R2ListPartsRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<R2ListMultipartUploadsResponse> ListMultipartUploadsAsync(
        R2ListMultipartUploadsRequest request, 
        CancellationToken cancellationToken = default);
}
```

### Request Models

#### R2InitiateMultipartUploadRequest
```csharp
public class R2InitiateMultipartUploadRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? ServerSideEncryption { get; set; }
    public string? StorageClass { get; set; }
    public string? CacheControl { get; set; }
    public string? ContentDisposition { get; set; }
    public string? ContentEncoding { get; set; }
    public DateTime? Expires { get; set; }
    public string? SSECustomerAlgorithm { get; set; }
    public string? SSECustomerKey { get; set; }
    public string? SSECustomerKeyMD5 { get; set; }
}
```

#### R2UploadPartRequest
```csharp
public class R2UploadPartRequest : IDisposable
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public int PartNumber { get; set; }
    public Stream? ContentStream { get; set; }
    public byte[]? ContentBytes { get; set; }
    public string? FilePath { get; set; }
    public string? ContentMD5 { get; set; }
    public string? SSECustomerAlgorithm { get; set; }
    public string? SSECustomerKey { get; set; }
    public string? SSECustomerKeyMD5 { get; set; }
}
```

#### R2CompleteMultipartUploadRequest
```csharp
public class R2CompleteMultipartUploadRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public List<R2CompletedPart> Parts { get; set; } = [];
}
```

#### R2CompletedPart
```csharp
public class R2CompletedPart
{
    public int PartNumber { get; set; }
    public required string ETag { get; set; }
}
```

#### R2AbortMultipartUploadRequest
```csharp
public class R2AbortMultipartUploadRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public string? ExpectedBucketOwner { get; set; }
}
```

#### R2ListPartsRequest
```csharp
public class R2ListPartsRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public int? MaxParts { get; set; }
    public int? PartNumberMarker { get; set; }
    public string? ExpectedBucketOwner { get; set; }
}
```

#### R2ListMultipartUploadsRequest
```csharp
public class R2ListMultipartUploadsRequest
{
    public required string BucketName { get; set; }
    public string? Prefix { get; set; }
    public string? Delimiter { get; set; }
    public int? MaxUploads { get; set; }
    public string? KeyMarker { get; set; }
    public string? UploadIdMarker { get; set; }
    public string? ExpectedBucketOwner { get; set; }
}
```

### Response Models

#### R2InitiateMultipartUploadResponse
```csharp
public class R2InitiateMultipartUploadResponse
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public string? ServerSideEncryption { get; set; }
    public DateTime InitiatedAt { get; set; }
}
```

#### R2UploadPartResponse
```csharp
public class R2UploadPartResponse
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public int PartNumber { get; set; }
    public required string ETag { get; set; }
    public string? ServerSideEncryption { get; set; }
    public DateTime UploadedAt { get; set; }
}
```

#### R2CompleteMultipartUploadResponse
```csharp
public class R2CompleteMultipartUploadResponse
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string ETag { get; set; }
    public string? Location { get; set; }
    public string? VersionId { get; set; }
    public string? ServerSideEncryption { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

#### R2AbortMultipartUploadResponse
```csharp
public class R2AbortMultipartUploadResponse
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public DateTime AbortedAt { get; set; }
}
```

#### R2ListPartsResponse
```csharp
public class R2ListPartsResponse
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public List<R2PartInfoResponse> Parts { get; set; } = [];
    public int? MaxParts { get; set; }
    public bool IsTruncated { get; set; }
    public int? NextPartNumberMarker { get; set; }
    public string? StorageClass { get; set; }
    public string? Owner { get; set; }
}
```

#### R2PartInfoResponse
```csharp
public class R2PartInfoResponse
{
    public int PartNumber { get; set; }
    public required string ETag { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
}
```

#### R2ListMultipartUploadsResponse
```csharp
public class R2ListMultipartUploadsResponse
{
    public required string BucketName { get; set; }
    public List<R2MultipartUploadInfoResponse> Uploads { get; set; } = [];
    public string? Prefix { get; set; }
    public string? Delimiter { get; set; }
    public int? MaxUploads { get; set; }
    public bool IsTruncated { get; set; }
    public string? NextKeyMarker { get; set; }
    public string? NextUploadIdMarker { get; set; }
    public List<string> CommonPrefixes { get; set; } = [];
}
```

#### R2MultipartUploadInfoResponse
```csharp
public class R2MultipartUploadInfoResponse
{
    public required string Key { get; set; }
    public required string UploadId { get; set; }
    public DateTime Initiated { get; set; }
    public string? StorageClass { get; set; }
    public string? Owner { get; set; }
}
```

## Best Practices

1. **Part Size**: Use parts between 5MB and 5GB (except the last part which can be smaller)
2. **Parallel Uploads**: Upload parts in parallel to maximize throughput
3. **Error Handling**: Always wrap operations in try-catch blocks and implement retry logic
4. **Resource Management**: Dispose `R2UploadPartRequest` when using streams
5. **ETag Storage**: Store ETags from each part upload for the completion step
6. **Cleanup**: Always abort incomplete uploads to avoid unnecessary storage charges
7. **Progress Tracking**: Implement progress tracking for large uploads
8. **Part Numbering**: Use sequential part numbers starting from 1

## Example: Complete Multipart Upload Manager

```csharp
public class MultipartUploadManager
{
    private readonly IR2Client _r2Client;
    private readonly ILogger<MultipartUploadManager> _logger;
    private const int DefaultPartSize = 10 * 1024 * 1024; // 10MB

    public MultipartUploadManager(IR2Client r2Client, ILogger<MultipartUploadManager> logger)
    {
        _r2Client = r2Client;
        _logger = logger;
    }

    public async Task<bool> UploadLargeFileAsync(
        string bucketName,
        string key,
        Stream fileStream,
        string contentType,
        int partSize = DefaultPartSize,
        int maxConcurrentParts = 5,
        CancellationToken cancellationToken = default)
    {
        string uploadId = string.Empty;
        var completedParts = new List<R2CompletedPart>();

        try
        {
            // Step 1: Initiate multipart upload
            var initiateRequest = new R2InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentType = contentType,
                Metadata = new Dictionary<string, string>
                {
                    { "upload-method", "multipart" },
                    { "upload-date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                }
            };

            var initiateResponse = await _r2Client.MultipartUploads
                .InitiateMultipartUploadAsync(initiateRequest, cancellationToken);
            
            uploadId = initiateResponse.UploadId;
            _logger.LogInformation("Initiated multipart upload {UploadId} for {Key}", uploadId, key);

            // Step 2: Upload parts
            var parts = await CreatePartsAsync(fileStream, partSize);
            _logger.LogInformation("Created {PartCount} parts for upload", parts.Count);

            using var semaphore = new SemaphoreSlim(maxConcurrentParts, maxConcurrentParts);
            var uploadTasks = parts.Select(async (part, index) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var partRequest = new R2UploadPartRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        UploadId = uploadId,
                        PartNumber = index + 1,
                        ContentBytes = part
                    };

                    var partResponse = await _r2Client.MultipartUploads
                        .UploadPartAsync(partRequest, cancellationToken);
                    
                    _logger.LogDebug("Uploaded part {PartNumber} with ETag {ETag}", 
                        partResponse.PartNumber, partResponse.ETag);
                    
                    return new R2CompletedPart 
                    { 
                        PartNumber = partResponse.PartNumber, 
                        ETag = partResponse.ETag 
                    };
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var uploadResults = await Task.WhenAll(uploadTasks);
            completedParts.AddRange(uploadResults.OrderBy(p => p.PartNumber));

            // Step 3: Complete multipart upload
            var completeRequest = new R2CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                Parts = completedParts
            };

            var completeResponse = await _r2Client.MultipartUploads
                .CompleteMultipartUploadAsync(completeRequest, cancellationToken);

            _logger.LogInformation("Completed multipart upload for {Key} with ETag {ETag}",
                completeResponse.Key, completeResponse.ETag);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload {Key} using multipart upload", key);

            // Cleanup: abort the upload if it was initiated
            if (!string.IsNullOrEmpty(uploadId))
            {
                try
                {
                    var abortRequest = new R2AbortMultipartUploadRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        UploadId = uploadId
                    };

                    await _r2Client.MultipartUploads.AbortMultipartUploadAsync(abortRequest, cancellationToken);
                    _logger.LogInformation("Aborted multipart upload {UploadId} due to failure", uploadId);
                }
                catch (Exception abortEx)
                {
                    _logger.LogError(abortEx, "Failed to abort multipart upload {UploadId}", uploadId);
                }
            }

            return false;
        }
    }

    public async Task<List<string>> CleanupIncompleteUploadsAsync(
        string bucketName,
        TimeSpan olderThan,
        CancellationToken cancellationToken = default)
    {
        var cleanedUploadIds = new List<string>();
        var cutoffDate = DateTime.UtcNow - olderThan;

        try
        {
            var listRequest = new R2ListMultipartUploadsRequest
            {
                BucketName = bucketName
            };

            var uploads = await _r2Client.MultipartUploads
                .ListMultipartUploadsAsync(listRequest, cancellationToken);

            var oldUploads = uploads.Uploads
                .Where(upload => upload.Initiated < cutoffDate)
                .ToList();

            _logger.LogInformation("Found {Count} incomplete uploads older than {CutoffDate}",
                oldUploads.Count, cutoffDate);

            foreach (var upload in oldUploads)
            {
                try
                {
                    var abortRequest = new R2AbortMultipartUploadRequest
                    {
                        BucketName = bucketName,
                        Key = upload.Key,
                        UploadId = upload.UploadId
                    };

                    await _r2Client.MultipartUploads.AbortMultipartUploadAsync(abortRequest, cancellationToken);
                    cleanedUploadIds.Add(upload.UploadId);

                    _logger.LogDebug("Cleaned up old upload {UploadId} for key {Key}",
                        upload.UploadId, upload.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cleanup upload {UploadId} for key {Key}",
                        upload.UploadId, upload.Key);
                }
            }

            _logger.LogInformation("Cleaned up {Count} incomplete uploads", cleanedUploadIds.Count);
            return cleanedUploadIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup incomplete uploads in bucket {BucketName}", bucketName);
            return cleanedUploadIds;
        }
    }

    private static async Task<List<byte[]>> CreatePartsAsync(Stream stream, int partSize)
    {
        var parts = new List<byte[]>();
        var buffer = new byte[partSize];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            if (bytesRead < partSize)
            {
                // Last part - resize buffer
                var lastPart = new byte[bytesRead];
                Array.Copy(buffer, lastPart, bytesRead);
                parts.Add(lastPart);
            }
            else
            {
                parts.Add((byte[])buffer.Clone());
            }
        }

        return parts;
    }
}
```