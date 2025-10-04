# Ebee.Cloudflare.R2

A comprehensive .NET client library for Cloudflare R2 storage, providing a simple and intuitive API for managing buckets, objects, signed URLs, and multipart uploads.

[![NuGet Version](https://img.shields.io/nuget/v/Ebee.Cloudflare.R2.svg)](https://www.nuget.org/packages/Ebee.Cloudflare.R2/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ebee.Cloudflare.R2.svg)](https://www.nuget.org/packages/Ebee.Cloudflare.R2/)
[![License](https://img.shields.io/github/license/ebeeraheem/Ebee.Cloudflare.R2)](LICENSE)

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Usage Examples](#usage-examples)
  - [Bucket Operations](#bucket-operations)
  - [Object Operations](#object-operations)
  - [Signed URLs](#signed-urls)
  - [Multipart Uploads](#multipart-uploads)
- [API Reference](#api-reference)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Bucket Management**: Create, list, and delete R2 buckets
- **Object Operations**: Upload, download, copy, delete, and list objects
- **Metadata Support**: Full support for custom metadata and headers
- **Signed URLs**: Generate pre-signed URLs for secure access
- **Multipart Uploads**: Handle large file uploads efficiently
- **Encryption**: Support for server-side encryption (SSE-S3, SSE-C)
- **Async/Await**: Full asynchronous support throughout
- **Strongly Typed**: Rich models with comprehensive validation
- **Error Handling**: Detailed exception handling with R2-specific errors
- **Dependency Injection**: Built-in DI container support

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Ebee.Cloudflare.R2
```

Or via Package Manager Console:

```powershell
Install-Package Ebee.Cloudflare.R2
```

## Quick Start

### 1. Register Services

```csharp
using Ebee.Cloudflare.R2;

// In Program.cs or Startup.cs
services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key-id";
    options.SecretAccessKey = "your-secret-access-key";
    options.AccountId = "your-account-id";
});
```

### 2. Inject and Use

```csharp
public class FileService
{
    private readonly IR2Client _r2Client;

    public FileService(IR2Client r2Client)
    {
        _r2Client = r2Client;
    }

    public async Task<string> UploadFileAsync(string bucketName, string key, Stream content)
    {
        var request = new R2PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentStream = content,
            ContentType = "application/octet-stream"
        };

        var response = await _r2Client.Objects.PutObjectAsync(request);
        return response.ETag;
    }
}
```

## Configuration

### Basic Configuration

```csharp
services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key-id";
    options.SecretAccessKey = "your-secret-access-key";
    options.AccountId = "your-account-id";
});
```

### Environment Variables

You can also configure using environment variables:

```bash
R2_ACCESS_KEY_ID=your-access-key-id
R2_SECRET_ACCESS_KEY=your-secret-access-key
R2_ACCOUNT_ID=your-account-id
```

## Usage Examples

### Bucket Operations

#### List Buckets

```csharp
var response = await _r2Client.Buckets.ListBucketsAsync();
foreach (var bucket in response.Buckets)
{
    Console.WriteLine($"Bucket: {bucket.Name}, Created: {bucket.CreationDate}");
}
```

#### Create Bucket

```csharp
var request = new R2CreateBucketRequest
{
    BucketName = "my-new-bucket"
};

var response = await _r2Client.Buckets.CreateBucketAsync(request);
Console.WriteLine($"Created bucket: {response.BucketName} at {response.Location}");
```

#### Delete Bucket

```csharp
var request = new R2DeleteBucketRequest
{
    BucketName = "bucket-to-delete"
};

await _r2Client.Buckets.DeleteBucketAsync(request);
```

### Object Operations

#### Upload Object from Byte Array

```csharp
var content = Encoding.UTF8.GetBytes("Hello, Cloudflare R2!");
var request = new R2PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "hello.txt",
    ContentBytes = content,
    ContentType = "text/plain",
    Metadata = new Dictionary<string, string>
    {
        { "author", "John Doe" },
        { "purpose", "example" }
    }
};

var response = await _r2Client.Objects.PutObjectAsync(request);
Console.WriteLine($"Uploaded with ETag: {response.ETag}");
```

#### Upload Object from Stream

```csharp
using var fileStream = File.OpenRead("document.pdf");
var request = new R2PutObjectRequest
{
    BucketName = "my-bucket",
    Key = "documents/document.pdf",
    ContentStream = fileStream,
    ContentType = "application/pdf"
};

var response = await _r2Client.Objects.PutObjectAsync(request);
```

#### Upload Object from File Path

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

#### Download Object

```csharp
var request = new R2GetObjectRequest
{
    BucketName = "my-bucket",
    Key = "document.pdf"
};

using var response = await _r2Client.Objects.GetObjectAsync(request);

// Access as byte array
byte[] content = response.ContentBytes;

// Or access as stream
using var contentStream = response.ContentStream;
await contentStream.CopyToAsync(outputStream);
```

#### List Objects

```csharp
var request = new R2ListObjectsRequest
{
    BucketName = "my-bucket",
    Prefix = "documents/",
    MaxKeys = 100
};

var response = await _r2Client.Objects.ListObjectsAsync(request);
foreach (var obj in response.Objects)
{
    Console.WriteLine($"Key: {obj.Key}, Size: {obj.Size} bytes");
}
```

#### Copy Object

```csharp
var request = new R2CopyObjectRequest
{
    SourceBucketName = "source-bucket",
    SourceKey = "original.txt",
    DestinationBucketName = "destination-bucket",
    DestinationKey = "copy.txt"
};

var response = await _r2Client.Objects.CopyObjectAsync(request);
```

#### Delete Object

```csharp
var request = new R2DeleteObjectRequest
{
    BucketName = "my-bucket",
    Key = "file-to-delete.txt"
};

await _r2Client.Objects.DeleteObjectAsync(request);
```

### Signed URLs

#### Generate GET Signed URL

```csharp
var request = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "document.pdf",
    ExpiresIn = TimeSpan.FromHours(1) // URL expires in 1 hour
};

var response = _r2Client.SignedUrls.GenerateGetSignedUrl(request);
Console.WriteLine($"Download URL: {response.SignedUrl}");
```

#### Generate PUT Signed URL

```csharp
var request = new R2GeneratePutSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "upload.jpg",
    ContentType = "image/jpeg",
    ExpiresIn = TimeSpan.FromMinutes(30)
};

var response = _r2Client.SignedUrls.GeneratePutSignedUrl(request);
Console.WriteLine($"Upload URL: {response.SignedUrl}");
```

#### Generate DELETE Signed URL

```csharp
var request = new R2GenerateDeleteSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "file-to-delete.txt",
    ExpiresIn = TimeSpan.FromMinutes(15)
};

var response = _r2Client.SignedUrls.GenerateDeleteSignedUrl(request);
Console.WriteLine($"Delete URL: {response.SignedUrl}");
```

### Multipart Uploads

#### Complete Multipart Upload Example

```csharp
// 1. Initiate multipart upload
var initiateRequest = new R2InitiateMultipartUploadRequest
{
    BucketName = "my-bucket",
    Key = "large-file.zip",
    ContentType = "application/zip"
};

var initiateResponse = await _r2Client.MultipartUploads.InitiateMultipartUploadAsync(initiateRequest);
var uploadId = initiateResponse.UploadId;

var completedParts = new List<R2CompletedPart>();

try
{
    // 2. Upload parts (minimum 5MB per part, except the last part)
    var partData = new byte[5 * 1024 * 1024]; // 5MB
    var partNumber = 1;

    var uploadPartRequest = new R2UploadPartRequest
    {
        BucketName = "my-bucket",
        Key = "large-file.zip",
        UploadId = uploadId,
        PartNumber = partNumber,
        ContentBytes = partData
    };

    var partResponse = await _r2Client.MultipartUploads.UploadPartAsync(uploadPartRequest);
    
    completedParts.Add(new R2CompletedPart
    {
        PartNumber = partNumber,
        ETag = partResponse.ETag
    });

    // 3. Complete the multipart upload
    var completeRequest = new R2CompleteMultipartUploadRequest
    {
        BucketName = "my-bucket",
        Key = "large-file.zip",
        UploadId = uploadId,
        Parts = completedParts
    };

    var completeResponse = await _r2Client.MultipartUploads.CompleteMultipartUploadAsync(completeRequest);
    Console.WriteLine($"Upload completed with ETag: {completeResponse.ETag}");
}
catch (Exception)
{
    // 4. Abort multipart upload on error
    var abortRequest = new R2AbortMultipartUploadRequest
    {
        BucketName = "my-bucket",
        Key = "large-file.zip",
        UploadId = uploadId
    };

    await _r2Client.MultipartUploads.AbortMultipartUploadAsync(abortRequest);
    throw;
}
```

## API Reference

### IR2Client Interface

```csharp
public interface IR2Client
{
    IBucketsClient Buckets { get; }
    IObjectsClient Objects { get; }
    ISignedUrlsClient SignedUrls { get; }
    IMultipartUploadsClient MultipartUploads { get; }
}
```

### Bucket Operations

| Method | Description |
|--------|-------------|
| `ListBucketsAsync()` | Lists all buckets in the account |
| `CreateBucketAsync(request)` | Creates a new bucket |
| `DeleteBucketAsync(request)` | Deletes an existing bucket |

### Object Operations

| Method | Description |
|--------|-------------|
| `ListObjectsAsync(request)` | Lists objects in a bucket |
| `GetObjectAsync(request)` | Downloads an object |
| `PutObjectAsync(request)` | Uploads an object |
| `DeleteObjectAsync(request)` | Deletes an object |
| `GetObjectMetadataAsync(request)` | Gets object metadata without downloading content |
| `CopyObjectAsync(request)` | Copies an object within or between buckets |

### Signed URL Operations

| Method | Description |
|--------|-------------|
| `GenerateGetSignedUrl(request)` | Generates a pre-signed URL for downloading |
| `GeneratePutSignedUrl(request)` | Generates a pre-signed URL for uploading |
| `GenerateDeleteSignedUrl(request)` | Generates a pre-signed URL for deleting |

### Multipart Upload Operations

| Method | Description |
|--------|-------------|
| `InitiateMultipartUploadAsync(request)` | Initiates a multipart upload |
| `UploadPartAsync(request)` | Uploads a part of a multipart upload |
| `CompleteMultipartUploadAsync(request)` | Completes a multipart upload |
| `AbortMultipartUploadAsync(request)` | Aborts a multipart upload |
| `ListPartsAsync(request)` | Lists parts of a multipart upload |
| `ListMultipartUploadsAsync(request)` | Lists ongoing multipart uploads |

## Error Handling

All operations may throw `R2Exception` with detailed error information:

```csharp
try
{
    var response = await _r2Client.Objects.GetObjectAsync(request);
}
catch (R2Exception ex)
{
    Console.WriteLine($"R2 operation failed: {ex.Message}");
    
    // Check inner exception for AWS S3 specific details
    if (ex.InnerException is AmazonS3Exception s3Ex)
    {
        Console.WriteLine($"S3 Error Code: {s3Ex.ErrorCode}");
        Console.WriteLine($"Status Code: {s3Ex.StatusCode}");
    }
}
```

Common error scenarios:
- **NoSuchBucket**: The specified bucket doesn't exist
- **NoSuchKey**: The specified object doesn't exist
- **BucketAlreadyExists**: Bucket name is already taken
- **BucketNotEmpty**: Cannot delete non-empty bucket
- **InvalidArgument**: Invalid request parameters

## Best Practices

### 1. Resource Management
```csharp
// Always dispose R2GetObjectResponse to free up streams
using var response = await _r2Client.Objects.GetObjectAsync(request);
// Use the response...
```

### 2. Content Sources
```csharp
// Use only one content source per request
var request = new R2PutObjectRequest
{
    BucketName = "bucket",
    Key = "key",
    ContentStream = stream, // Don't set ContentBytes or FilePath
    ContentType = "application/octet-stream"
};
```

### 3. Metadata Management
```csharp
// Use descriptive metadata keys
var metadata = new Dictionary<string, string>
{
    { "uploaded-by", "application-name" },
    { "content-category", "documents" },
    { "created-date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
};
```

### 4. Large File Handling
```csharp
// Use multipart uploads for files larger than 100MB
if (fileSize > 100 * 1024 * 1024) // 100MB
{
    // Use multipart upload
}
else
{
    // Use regular upload
}
```

### 5. Pagination
```csharp
// Handle pagination when listing large numbers of objects
var allObjects = new List<R2ObjectInfoResponse>();
string? continuationToken = null;

do
{
    var request = new R2ListObjectsRequest
    {
        BucketName = "my-bucket",
        MaxKeys = 1000,
        ContinuationToken = continuationToken
    };

    var response = await _r2Client.Objects.ListObjectsAsync(request);
    allObjects.AddRange(response.Objects);
    continuationToken = response.NextContinuationToken;
    
} while (!string.IsNullOrEmpty(continuationToken));
```

## Requirements

- .NET 8.0 or later
- Valid Cloudflare R2 credentials

## Getting R2 Credentials

1. Log in to your [Cloudflare Dashboard](https://dash.cloudflare.com/)
2. Navigate to **R2 Object Storage**
3. Go to **Manage R2 API tokens**
4. Create a new API token with appropriate permissions
5. Note your Account ID from the R2 dashboard

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [Documentation](docs/)
- [Issue Tracker](https://github.com/ebeeraheem/Ebee.Cloudflare.R2/issues)
- [Discussions](https://github.com/ebeeraheem/Ebee.Cloudflare.R2/discussions)

## Acknowledgments

- Built on top of the [AWS SDK for .NET](https://github.com/aws/aws-sdk-net)
- Inspired by the Cloudflare R2 API
- Special thanks to all contributors

---

**Happy coding with Cloudflare R2!**