# Ebee.Cloudflare.R2

A comprehensive .NET client library for Cloudflare R2 storage, providing a simple and intuitive API for managing buckets, objects, signed URLs, and multipart uploads.

[![NuGet Version](https://img.shields.io/nuget/v/Ebee.Cloudflare.R2.svg)](https://www.nuget.org/packages/Ebee.Cloudflare.R2/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ebee.Cloudflare.R2.svg)](https://www.nuget.org/packages/Ebee.Cloudflare.R2/)
[![License](https://img.shields.io/github/license/ebeeraheem/Ebee.Cloudflare.R2)](LICENSE)

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

```bash
dotnet add package Ebee.Cloudflare.R2
```

## Quick Start

### 1. Configure Services

```csharp
using Ebee.Cloudflare.R2;

services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key-id";
    options.SecretAccessKey = "your-secret-access-key";
    options.AccountId = "your-account-id";
});
```

### 2. Use the Client

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

        var response = await _r2Client.PutObjectAsync(request);
        return response.ETag;
    }
}
```

## Documentation

For detailed documentation and usage examples, please refer to the following topics:

- **Buckets**: Create, list, and delete R2 buckets - [Documentation](docs/buckets.md) • [Sample](samples/Ebee.Cloudflare.R2.Buckets)
- **Objects**: Upload, download, and manage objects - [Documentation](docs/objects.md) • [Sample](samples/Ebee.Cloudflare.R2.Objects)
- **Signed URLs**: Generate pre-signed URLs for secure access - [Documentation](docs/signedurls.md) • [Sample](samples/Ebee.Cloudflare.R2.SignedUrls)
- **Multipart Uploads**: Handle large file uploads efficiently - [Documentation](docs/multipartuploads.md) • [Sample](samples/Ebee.Cloudflare.R2.MultipartUploads)

## API Overview

```csharp
public interface IR2Client
{
    IBucketsClient Buckets { get; }           // Bucket operations
    IObjectsClient Objects { get; }           // Object operations  
    ISignedUrlsClient SignedUrls { get; }     // Pre-signed URLs
    IMultipartUploadsClient MultipartUploads { get; } // Large file uploads
}
```

## Configuration Options

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
```bash
R2_ACCESS_KEY_ID=your-access-key-id
R2_SECRET_ACCESS_KEY=your-secret-access-key
R2_ACCOUNT_ID=your-account-id
```

## Error Handling

All operations may throw `R2Exception` with detailed error information:

```csharp
try
{
    var response = await _r2Client.GetObjectAsync(request);
}
catch (R2Exception ex)
{
    Console.WriteLine($"R2 operation failed: {ex.Message}");
    
    // Check inner exception for AWS S3 specific details
    if (ex.InnerException is AmazonS3Exception s3Ex)
    {
        Console.WriteLine($"S3 Error Code: {s3Ex.ErrorCode}");
    }
}
```

## Getting R2 Credentials

1. Log in to your [Cloudflare Dashboard](https://dash.cloudflare.com/)
2. Navigate to **R2 Object Storage**
3. Go to **Manage R2 API tokens**
4. Create a new API token with appropriate permissions
5. Note your Account ID from the R2 dashboard

## Examples Repository

Check out the [`samples/`](samples/) directory for complete working examples:

- [**Bucket Management**](samples/Ebee.Cloudflare.R2.Buckets/Program.cs) - Create, list, delete buckets
- [**Object Operations**](samples/Ebee.Cloudflare.R2.Objects/Program.cs) - Upload, download, copy, delete objects
- [**Signed URLs**](samples/Ebee.Cloudflare.R2.SignedUrls/Program.cs) - Generate pre-signed URLs
- [**Multipart Uploads**](samples/Ebee.Cloudflare.R2.MultipartUploads/Program.cs) - Handle large file uploads

## Requirements

- .NET 8.0 or later
- Valid Cloudflare R2 credentials

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [Documentation](docs/)
- [Issue Tracker](https://github.com/ebeeraheem/Ebee.Cloudflare.R2/issues)
- [Discussions](https://github.com/ebeeraheem/Ebee.Cloudflare.R2/discussions)

---

**Happy coding with Cloudflare R2!**