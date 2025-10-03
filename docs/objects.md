# Objects

The `ObjectsClient` provides methods for managing R2 objects, including listing, uploading, downloading, copying, deleting objects, and retrieving object metadata.

## Getting Started

The `ObjectsClient` is automatically registered when you configure the R2 client in your dependency injection container:

```csharp
services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key";
    options.SecretAccessKey = "your-secret-key";
    options.AccountId = "your-account-id";
});
```

Then inject `IR2Client` and access the objects client:

```csharp
public class MyService
{
    private readonly IR2Client _r2Client;

    public MyService(IR2Client r2Client)
    {
        _r2Client = r2Client;
    }

    public async Task ManageObjects()
    {
        var objectsClient = _r2Client.Objects;
        // Use objects client methods...
    }
}
```

## Listing Objects

Retrieve a list of objects in a bucket with optional filtering and pagination:

```csharp
var request = new R2ListObjectsRequest
{
    BucketName = "my-bucket",
    Prefix = "documents/",           // Optional: filter by prefix
    Delimiter = "/",                 // Optional: group by delimiter
    MaxKeys = 100,                   // Optional: limit results (default 1000)
    ContinuationToken = null,        // Optional: for pagination
    FetchOwner = true,               // Optional: include owner info
    StartAfter = "documents/file1"   // Optional: start listing after this key
};

var response = await _r2Client.Objects.ListObjectsAsync(request);

Console.WriteLine($"Found {response.KeyCount} objects in bucket {response.BucketName}");
foreach (var obj in response.Objects)
{
    Console.WriteLine($"Key: {obj.Key}, Size: {obj.Size} bytes, Modified: {obj.LastModified}");
}

// Handle pagination
if (response.IsTruncated)
{
    var nextRequest = request with { ContinuationToken = response.NextContinuationToken };
    var nextPage = await _r2Client.Objects.ListObjectsAsync(nextRequest);
}
```

### Response Details

The `ListObjectsAsync` method returns an `R2ListObjectsResponse` containing:

- `Objects`: List of `R2ObjectInfoResponse` with object details
- `KeyCount`: Number of objects returned
- `IsTruncated`: Whether there are more results
- `NextContinuationToken`: Token for pagination
- `CommonPrefixes`: Common prefixes when using delimiter

## Uploading Objects

Upload objects to R2 using different content sources:

### Upload from Byte Array

```csharp
var contentBytes = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
var request = new R2PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "hello.txt",
    ContentBytes = contentBytes,
    ContentType = "text/plain",
    Metadata = new Dictionary<string, string>
    {
        { "author", "John Doe" },
        { "category", "greeting" }
    }
};

var response = await _r2Client.Objects.PutObjectAsync(request);
Console.WriteLine($"Uploaded {response.Key} with ETag: {response.ETag}");
```

### Upload from Stream

```csharp
using var fileStream = File.OpenRead("document.pdf");
var request = new R2PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "documents/document.pdf",
    ContentStream = fileStream,
    ContentType = "application/pdf",
    ServerSideEncryption = "AES256",
    StorageClass = "STANDARD"
};

var response = await _r2Client.Objects.PutObjectAsync(request);
```

### Upload from File Path

```csharp
var request = new R2PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "images/photo.jpg",
    FilePath = @"C:\Photos\vacation.jpg",
    ContentType = "image/jpeg"
};

var response = await _r2Client.Objects.PutObjectAsync(request);
```

### Server-Side Encryption with Customer Keys

```csharp
var request = new R2PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "sensitive-data.txt",
    ContentBytes = sensitiveData,
    SSECustomerAlgorithm = "AES256",
    SSECustomerKey = "your-base64-encoded-key",
    SSECustomerKeyMD5 = "md5-hash-of-key"
};

var response = await _r2Client.Objects.PutObjectAsync(request);
```

## Downloading Objects

Download objects from R2 with various options:

### Basic Download

```csharp
var request = new R2GetObjectRequest
{
    BucketName = "my-bucket",
    Key = "document.pdf"
};

using var response = await _r2Client.Objects.GetObjectAsync(request);

// Access content as byte array
byte[] content = response.ContentBytes;

// Or access as stream
using var contentStream = response.ContentStream;
await contentStream.CopyToAsync(outputStream);

Console.WriteLine($"Downloaded {response.Key} ({response.ContentLength} bytes)");
Console.WriteLine($"Content-Type: {response.ContentType}");
Console.WriteLine($"Last Modified: {response.LastModified}");
```

### Conditional Downloads

```csharp
var request = new R2GetObjectRequest
{
    BucketName = "my-bucket",
    Key = "document.pdf",
    IfMatch = "\"specific-etag\"",                    // Download only if ETag matches
    IfNoneMatch = "\"another-etag\"",                // Download only if ETag doesn't match
    IfModifiedSince = DateTime.UtcNow.AddDays(-7),   // Download only if modified since date
    IfUnmodifiedSince = DateTime.UtcNow.AddDays(-1)  // Download only if not modified since date
};

using var response = await _r2Client.Objects.GetObjectAsync(request);
```

### Range Downloads (Partial Content)

```csharp
var request = new R2GetObjectRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    Range = "bytes=0-1023"  // Download first 1024 bytes
};

using var response = await _r2Client.Objects.GetObjectAsync(request);
```

### Working with Metadata

```csharp
using var response = await _r2Client.Objects.GetObjectAsync(request);

// Access standard metadata
Console.WriteLine($"Content-Type: {response.ContentType}");
Console.WriteLine($"Content-Length: {response.ContentLength}");
Console.WriteLine($"ETag: {response.ETag}");
Console.WriteLine($"Cache-Control: {response.CacheControl}");

// Access custom metadata
foreach (var metadata in response.Metadata)
{
    Console.WriteLine($"Metadata {metadata.Key}: {metadata.Value}");
}
```

## Getting Object Metadata

Retrieve object metadata without downloading the content:

```csharp
var request = new R2GetObjectMetadataRequest
{
    BucketName = "my-bucket",
    Key = "document.pdf",
    VersionId = "specific-version-id"  // Optional: get metadata for specific version
};

var response = await _r2Client.Objects.GetObjectMetadataAsync(request);

Console.WriteLine($"Object: {response.Key}");
Console.WriteLine($"Size: {response.ContentLength} bytes");
Console.WriteLine($"Last Modified: {response.LastModified}");
Console.WriteLine($"Storage Class: {response.StorageClass}");
Console.WriteLine($"Server-Side Encryption: {response.ServerSideEncryption}");
```

## Copying Objects

Copy objects within R2 or between buckets:

### Simple Copy

```csharp
var request = new R2CopyObjectRequest
{
    SourceBucketName = "source-bucket",
    SourceKey = "original-file.txt",
    DestinationBucketName = "destination-bucket",
    DestinationKey = "copied-file.txt"
};

var response = await _r2Client.Objects.CopyObjectAsync(request);
Console.WriteLine($"Copied to {response.DestinationKey} with ETag: {response.ETag}");
```

### Copy with Metadata Changes

```csharp
var request = new R2CopyObjectRequest
{
    SourceBucketName = "my-bucket",
    SourceKey = "original.txt",
    DestinationBucketName = "my-bucket",
    DestinationKey = "renamed.txt",
    MetadataDirective = "REPLACE",  // Replace metadata instead of copying
    ContentType = "text/plain",
    Metadata = new Dictionary<string, string>
    {
        { "updated-by", "copy-operation" },
        { "copy-date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
    }
};

var response = await _r2Client.Objects.CopyObjectAsync(request);
```

### Copy Specific Version

```csharp
var request = new R2CopyObjectRequest
{
    SourceBucketName = "versioned-bucket",
    SourceKey = "document.txt",
    SourceVersionId = "specific-version-id",
    DestinationBucketName = "backup-bucket",
    DestinationKey = "document-backup.txt"
};

var response = await _r2Client.Objects.CopyObjectAsync(request);
```

## Deleting Objects

Delete objects from R2:

### Simple Delete

```csharp
var request = new R2DeleteObjectRequest
{
    BucketName = "my-bucket",
    Key = "obsolete-file.txt"
};

var response = await _r2Client.Objects.DeleteObjectAsync(request);
Console.WriteLine($"Deleted {response.Key} at {response.DeletedAt}");
```

### Delete Specific Version

```csharp
var request = new R2DeleteObjectRequest
{
    BucketName = "versioned-bucket",
    Key = "document.txt",
    VersionId = "specific-version-id"
};

var response = await _r2Client.Objects.DeleteObjectAsync(request);
```

### Delete with Governance Bypass

```csharp
var request = new R2DeleteObjectRequest
{
    BucketName = "protected-bucket",
    Key = "protected-file.txt",
    BypassGovernanceRetention = true  // Bypass object lock governance mode
};

var response = await _r2Client.Objects.DeleteObjectAsync(request);
```

## Error Handling

All object operations may throw `R2Exception` with specific error scenarios:

```csharp
try
{
    var response = await _r2Client.Objects.GetObjectAsync(request);
    // Process successful response
}
catch (R2Exception ex)
{
    // Handle specific R2 errors
    Console.WriteLine($"R2 operation failed: {ex.Message}");
    
    // Check inner exception for AWS S3 specific details
    if (ex.InnerException is AmazonS3Exception s3Ex)
    {
        Console.WriteLine($"S3 Error Code: {s3Ex.ErrorCode}");
    }
}
```

Common error scenarios:
- **NoSuchBucket**: The specified bucket doesn't exist
- **NoSuchKey**: The specified object doesn't exist
- **InvalidArgument**: Invalid request parameters

## API Reference

### IObjectsClient Interface

```csharp
public interface IObjectsClient
{
    Task<R2ListObjectsResponse> ListObjectsAsync(R2ListObjectsRequest request, CancellationToken cancellationToken = default);
    Task<R2GetObjectResponse> GetObjectAsync(R2GetObjectRequest request, CancellationToken cancellationToken = default);
    Task<R2PutObjectResponse> PutObjectAsync(R2PutObjectRequest request, CancellationToken cancellationToken = default);
    Task<R2DeleteObjectResponse> DeleteObjectAsync(R2DeleteObjectRequest request, CancellationToken cancellationToken = default);
    Task<R2GetObjectMetadataResponse> GetObjectMetadataAsync(R2GetObjectMetadataRequest request, CancellationToken cancellationToken = default);
    Task<R2CopyObjectResponse> CopyObjectAsync(R2CopyObjectRequest request, CancellationToken cancellationToken = default);
}
```

### Request Models

#### R2ListObjectsRequest
```csharp
public class R2ListObjectsRequest
{
    public required string BucketName { get; set; }
    public string? Prefix { get; set; }
    public string? Delimiter { get; set; }
    public int? MaxKeys { get; set; }
    public string? ContinuationToken { get; set; }
    public bool FetchOwner { get; set; }
    public string? StartAfter { get; set; }
}
```

#### R2GetObjectRequest
```csharp
public class R2GetObjectRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public string? VersionId { get; set; }
    public string? Range { get; set; }
    public string? IfMatch { get; set; }
    public string? IfNoneMatch { get; set; }
    public DateTime? IfModifiedSince { get; set; }
    public DateTime? IfUnmodifiedSince { get; set; }
}
```

#### R2PutObjectRequest
```csharp
public class R2PutObjectRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public Stream? ContentStream { get; set; }
    public byte[]? ContentBytes { get; set; }
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? ServerSideEncryption { get; set; }
    public string? StorageClass { get; set; }
    public string? SSECustomerAlgorithm { get; set; }
    public string? SSECustomerKey { get; set; }
    public string? SSECustomerKeyMD5 { get; set; }
}
```

#### R2DeleteObjectRequest
```csharp
public class R2DeleteObjectRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public string? VersionId { get; set; }
    public bool BypassGovernanceRetention { get; set; }
    public string? ExpectedBucketOwner { get; set; }
}
```

#### R2CopyObjectRequest
```csharp
public class R2CopyObjectRequest
{
    public required string SourceBucketName { get; set; }
    public required string SourceKey { get; set; }
    public required string DestinationBucketName { get; set; }
    public required string DestinationKey { get; set; }
    public string? SourceVersionId { get; set; }
    public string? MetadataDirective { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? ServerSideEncryption { get; set; }
    public string? StorageClass { get; set; }
}
```

### Response Models

#### R2ListObjectsResponse
```csharp
public class R2ListObjectsResponse
{
    public required string BucketName { get; set; }
    public List<R2ObjectInfoResponse> Objects { get; set; } = [];
    public string? Prefix { get; set; }
    public string? Delimiter { get; set; }
    public int? MaxKeys { get; set; }
    public bool IsTruncated { get; set; }
    public string? NextContinuationToken { get; set; }
    public int KeyCount { get; set; }
    public List<string> CommonPrefixes { get; set; } = [];
}
```

#### R2ObjectInfoResponse
```csharp
public class R2ObjectInfoResponse
{
    public required string Key { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string? ETag { get; set; }
    public string? StorageClass { get; set; }
    public string? Owner { get; set; }
}
```

#### R2GetObjectResponse
```csharp
public class R2GetObjectResponse : IDisposable
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public byte[] ContentBytes { get; set; } = [];
    public Stream? ContentStream { get; set; }
    public string? ContentType { get; set; }
    public long ContentLength { get; set; }
    public string? ETag { get; set; }
    public DateTime LastModified { get; set; }
    public string? VersionId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string? CacheControl { get; set; }
    public string? ContentDisposition { get; set; }
    public string? ContentEncoding { get; set; }
    public DateTime? Expires { get; set; }
}
```

## Best Practices

1. **Resource Management**: Always dispose `R2GetObjectResponse` to free up streams and memory
2. **Content Sources**: Use only one content source per `PutObjectRequest` (ContentStream, ContentBytes, or FilePath)
3. **Metadata**: Use descriptive metadata keys and avoid sensitive information
4. **Error Handling**: Always wrap object operations in try-catch blocks
5. **Pagination**: Handle pagination properly when listing large numbers of objects
6. **Content Types**: Set appropriate `ContentType` for better browser handling
7. **Memory Management**: For large files, prefer streaming over loading entire content into memory

## Example: Complete Object Manager

```csharp
public class ObjectManager
{
    private readonly IR2Client _r2Client;
    private readonly ILogger<ObjectManager> _logger;

    public ObjectManager(IR2Client r2Client, ILogger<ObjectManager> logger)
    {
        _r2Client = r2Client;
        _logger = logger;
    }

    public async Task<bool> UploadFileAsync(string bucketName, string key, Stream content, string contentType)
    {
        try
        {
            var request = new R2PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentStream = content,
                ContentType = contentType,
                Metadata = new Dictionary<string, string>
                {
                    { "uploaded-by", "object-manager" },
                    { "upload-date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                }
            };

            var response = await _r2Client.Objects.PutObjectAsync(request);
            _logger.LogInformation("Uploaded object {Key} to bucket {BucketName} with ETag {ETag}",
                response.Key, response.BucketName, response.ETag);
            
            return true;
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to upload object {Key} to bucket {BucketName}", key, bucketName);
            return false;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string bucketName, string key)
    {
        try
        {
            var request = new R2GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _r2Client.Objects.GetObjectAsync(request);
            _logger.LogInformation("Downloaded object {Key} from bucket {BucketName} ({ContentLength} bytes)",
                response.Key, response.BucketName, response.ContentLength);
            
            return response.ContentStream;
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to download object {Key} from bucket {BucketName}", key, bucketName);
            return null;
        }
    }

    public async Task<List<string>> ListObjectKeysAsync(string bucketName, string? prefix = null)
    {
        var allKeys = new List<string>();
        string? continuationToken = null;

        try
        {
            do
            {
                var request = new R2ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = prefix,
                    MaxKeys = 1000,
                    ContinuationToken = continuationToken
                };

                var response = await _r2Client.Objects.ListObjectsAsync(request);
                allKeys.AddRange(response.Objects.Select(obj => obj.Key));
                
                continuationToken = response.NextContinuationToken;
                
            } while (!string.IsNullOrEmpty(continuationToken));

            _logger.LogInformation("Listed {Count} objects from bucket {BucketName} with prefix '{Prefix}'",
                allKeys.Count, bucketName, prefix ?? "none");
            
            return allKeys;
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects in bucket {BucketName}", bucketName);
            return [];
        }
    }
}
```