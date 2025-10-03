# Signed URLs

The `SignedUrlsClient` provides methods for generating pre-signed URLs that allow temporary access to R2 objects without requiring authentication credentials. These URLs are perfect for secure file sharing, direct uploads from browsers, and temporary access control.

## Getting Started

The `SignedUrlsClient` is automatically registered when you configure the R2 client in your dependency injection container:

```csharp
services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key";
    options.SecretAccessKey = "your-secret-key";
    options.AccountId = "your-account-id";
});
```

Then inject `IR2Client` and access the signed URLs client:

```csharp
public class MyService
{
    private readonly IR2Client _r2Client;

    public MyService(IR2Client r2Client)
    {
        _r2Client = r2Client;
    }

    public async Task GenerateSignedUrls()
    {
        var signedUrlsClient = _r2Client.SignedUrls;
        // Use signed URLs client methods...
    }
}
```

## Generating GET Signed URLs

Create pre-signed URLs that allow temporary read access to objects:

### Basic GET URL

```csharp
var request = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "documents/report.pdf",
    ExpiresIn = TimeSpan.FromHours(2)  // URL expires in 2 hours
};

var response = _r2Client.SignedUrls.GenerateGetSignedUrl(request);

Console.WriteLine($"Signed URL: {response.SignedUrl}");
Console.WriteLine($"Expires at: {response.ExpiresAt}");
Console.WriteLine($"HTTP Method: {response.HttpMethod}");
```

### GET URL with Response Headers

Control how browsers handle the downloaded file:

```csharp
var request = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "images/photo.jpg",
    ExpiresIn = TimeSpan.FromMinutes(30),
    ResponseContentType = "image/jpeg",
    ResponseContentDisposition = "attachment; filename=\"vacation-photo.jpg\"",
    ResponseCacheControl = "max-age=3600, must-revalidate",
    ResponseExpires = DateTime.UtcNow.AddHours(1)
};

var response = _r2Client.SignedUrls.GenerateGetSignedUrl(request);
```

### GET URL for Specific Version

Access a specific version of a versioned object:

```csharp
var request = new R2GenerateGetSignedUrlRequest
{
    BucketName = "versioned-bucket",
    Key = "document.txt",
    VersionId = "version-123",
    ExpiresIn = TimeSpan.FromDays(1)
};

var response = _r2Client.SignedUrls.GenerateGetSignedUrl(request);
```

### Expiration Options

You can specify expiration in multiple ways:

```csharp
// Option 1: Using ExpiresIn (duration from now)
var request1 = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "file.txt",
    ExpiresIn = TimeSpan.FromHours(6)
};

// Option 2: Using explicit Expires (absolute time)
var request2 = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "file.txt",
    Expires = DateTime.UtcNow.AddDays(2)
};

// Option 3: Using default expiration (1 hour)
var request3 = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "file.txt"
    // No expiration specified - defaults to 1 hour
};
```

## Generating PUT Signed URLs

Create pre-signed URLs that allow temporary upload access:

### Basic PUT URL

```csharp
var request = new R2GeneratePutSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "uploads/new-document.pdf",
    ContentType = "application/pdf",
    ExpiresIn = TimeSpan.FromMinutes(15)
};

var response = _r2Client.SignedUrls.GeneratePutSignedUrl(request);

// Client can now upload directly to this URL
Console.WriteLine($"Upload URL: {response.SignedUrl}");
```

### PUT URL with Metadata and Headers

Include metadata and specific headers for the upload:

```csharp
var request = new R2GeneratePutSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "documents/contract.pdf",
    ContentType = "application/pdf",
    CacheControl = "max-age=86400",
    ContentDisposition = "inline; filename=\"contract.pdf\"",
    ContentEncoding = "gzip",
    ServerSideEncryption = "AES256",
    StorageClass = "STANDARD",
    Metadata = new Dictionary<string, string>
    {
        { "author", "John Doe" },
        { "department", "Legal" },
        { "document-type", "contract" },
        { "upload-date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
    }
};

var response = _r2Client.SignedUrls.GeneratePutSignedUrl(request);
```

### PUT URL for Versioned Upload

Upload to a bucket with versioning enabled:

```csharp
var request = new R2GeneratePutSignedUrlRequest
{
    BucketName = "versioned-bucket",
    Key = "documents/policy.pdf",
    ContentType = "application/pdf",
    VersionId = "new-version-id",  // Optional: for specific version
    ExpiresIn = TimeSpan.FromHours(1)
};

var response = _r2Client.SignedUrls.GeneratePutSignedUrl(request);
```

## Generating DELETE Signed URLs

Create pre-signed URLs that allow temporary delete access:

### Basic DELETE URL

```csharp
var request = new R2GenerateDeleteSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "temporary-files/temp.txt",
    ExpiresIn = TimeSpan.FromMinutes(10)
};

var response = _r2Client.SignedUrls.GenerateDeleteSignedUrl(request);

Console.WriteLine($"Delete URL: {response.SignedUrl}");
Console.WriteLine($"Expires at: {response.ExpiresAt}");
```

### DELETE URL with Governance Bypass

Delete objects protected by governance mode retention:

```csharp
var request = new R2GenerateDeleteSignedUrlRequest
{
    BucketName = "protected-bucket",
    Key = "protected-document.pdf",
    BypassGovernanceRetention = true,
    ExpectedBucketOwner = "expected-owner-id",
    ExpiresIn = TimeSpan.FromMinutes(5)
};

var response = _r2Client.SignedUrls.GenerateDeleteSignedUrl(request);
```

### DELETE URL for Specific Version

Delete a specific version of a versioned object:

```csharp
var request = new R2GenerateDeleteSignedUrlRequest
{
    BucketName = "versioned-bucket",
    Key = "document.txt",
    VersionId = "version-to-delete",
    ExpiresIn = TimeSpan.FromHours(1)
};

var response = _r2Client.SignedUrls.GenerateDeleteSignedUrl(request);
```

## Using Signed URLs

### Client-Side Upload Example (JavaScript)

```javascript
// Upload a file using a PUT signed URL
async function uploadFile(signedUrl, file) {
    const response = await fetch(signedUrl, {
        method: 'PUT',
        body: file,
        headers: {
            'Content-Type': file.type
        }
    });
    
    if (response.ok) {
        console.log('File uploaded successfully');
    } else {
        console.error('Upload failed:', response.statusText);
    }
}
```

### Client-Side Download Example (JavaScript)

```javascript
// Download a file using a GET signed URL
async function downloadFile(signedUrl, filename) {
    const response = await fetch(signedUrl);
    
    if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        window.URL.revokeObjectURL(url);
    } else {
        console.error('Download failed:', response.statusText);
    }
}
```

### HttpClient Usage (C#)

```csharp
// Upload using HttpClient
public async Task UploadUsingSignedUrl(string signedUrl, Stream fileStream, string contentType)
{
    using var httpClient = new HttpClient();
    using var content = new StreamContent(fileStream);
    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
    
    var response = await httpClient.PutAsync(signedUrl, content);
    response.EnsureSuccessStatusCode();
}

// Download using HttpClient
public async Task<byte[]> DownloadUsingSignedUrl(string signedUrl)
{
    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync(signedUrl);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadAsByteArrayAsync();
}
```

## Expiration Limits and Validation

### Expiration Rules

- **Default Expiration**: 1 hour if no expiration is specified
- **Maximum Expiration**: 7 days (168 hours)
- **Minimum Expiration**: Must be a positive duration
- **Time Zone Handling**: All times are converted to UTC

### Validation Examples

```csharp
// Valid expirations
var validRequest1 = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "file.txt",
    ExpiresIn = TimeSpan.FromMinutes(1)  // Minimum: 1 minute
};

var validRequest2 = new R2GenerateGetSignedUrlRequest
{
    BucketName = "my-bucket",
    Key = "file.txt",
    ExpiresIn = TimeSpan.FromDays(7)     // Maximum: 7 days
};

// Invalid expirations (will throw R2Exception)
try
{
    var invalidRequest = new R2GenerateGetSignedUrlRequest
    {
        BucketName = "my-bucket",
        Key = "file.txt",
        ExpiresIn = TimeSpan.FromDays(8)  // Too long - exceeds 7 days
    };
    
    var response = _r2Client.SignedUrls.GenerateGetSignedUrl(invalidRequest);
}
catch (R2Exception ex)
{
    Console.WriteLine($"Invalid expiration: {ex.Message}");
}
```

## Error Handling

All signed URL operations may throw `R2Exception` with specific error scenarios:

```csharp
try
{
    var response = _r2Client.SignedUrls.GenerateGetSignedUrl(request);
    // Use the signed URL
}
catch (R2Exception ex)
{
    Console.WriteLine($"Failed to generate signed URL: {ex.Message}");
    
    // Check inner exception for AWS S3 specific details
    if (ex.InnerException is AmazonS3Exception s3Ex)
    {
        Console.WriteLine($"S3 Error Code: {s3Ex.ErrorCode}");
    }
}
```

Common error scenarios:
- **InvalidArgument**: Invalid request parameters (empty bucket name, key, etc.)
- **ArgumentOutOfRangeException**: Invalid expiration times
- **S3Exception**: Underlying S3 service errors

## API Reference

### ISignedUrlsClient Interface

```csharp
public interface ISignedUrlsClient
{
    R2SignedUrlResponse GenerateGetSignedUrl(R2GenerateGetSignedUrlRequest request);
    R2SignedUrlResponse GeneratePutSignedUrl(R2GeneratePutSignedUrlRequest request);
    R2SignedUrlResponse GenerateDeleteSignedUrl(R2GenerateDeleteSignedUrlRequest request);
}
```

### Request Models

#### R2GenerateGetSignedUrlRequest
```csharp
public class R2GenerateGetSignedUrlRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public DateTime? Expires { get; set; }
    public TimeSpan? ExpiresIn { get; set; }
    public string? VersionId { get; set; }
    public string? ResponseContentType { get; set; }
    public string? ResponseContentDisposition { get; set; }
    public string? ResponseCacheControl { get; set; }
    public DateTime? ResponseExpires { get; set; }
}
```

#### R2GeneratePutSignedUrlRequest
```csharp
public class R2GeneratePutSignedUrlRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public DateTime? Expires { get; set; }
    public TimeSpan? ExpiresIn { get; set; }
    public string? VersionId { get; set; }
    public string? ContentType { get; set; }
    public string? CacheControl { get; set; }
    public string? ContentDisposition { get; set; }
    public string? ContentEncoding { get; set; }
    public string? ServerSideEncryption { get; set; }
    public string? StorageClass { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```

#### R2GenerateDeleteSignedUrlRequest
```csharp
public class R2GenerateDeleteSignedUrlRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public DateTime? Expires { get; set; }
    public TimeSpan? ExpiresIn { get; set; }
    public string? VersionId { get; set; }
    public bool BypassGovernanceRetention { get; set; }
    public string? ExpectedBucketOwner { get; set; }
}
```

### Response Model

#### R2SignedUrlResponse
```csharp
public class R2SignedUrlResponse
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
    public required string SignedUrl { get; set; }
    public required string HttpMethod { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? VersionId { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

## Security Best Practices

1. **Expiration Times**: Use the shortest expiration time possible for your use case
2. **HTTPS Only**: Always use signed URLs over HTTPS to prevent URL interception
3. **Content Type Validation**: Specify and validate content types for uploads
4. **Access Logging**: Monitor usage of signed URLs through CloudFlare R2 logs
5. **URL Distribution**: Distribute signed URLs securely and avoid logging them
6. **Browser Security**: Consider using Content Security Policy headers when serving signed URLs

## Best Practices

1. **Expiration Management**: Set appropriate expiration times based on use case
2. **Metadata**: Include relevant metadata for tracking and organization
3. **Content Types**: Always specify content types for better browser handling
4. **Error Handling**: Implement proper error handling for expired or invalid URLs
5. **URL Sharing**: Share signed URLs through secure channels only
6. **Monitoring**: Log signed URL generation for audit trails

## Example: Complete Signed URL Manager

```csharp
public class SignedUrlManager
{
    private readonly IR2Client _r2Client;
    private readonly ILogger<SignedUrlManager> _logger;

    public SignedUrlManager(IR2Client r2Client, ILogger<SignedUrlManager> logger)
    {
        _r2Client = r2Client;
        _logger = logger;
    }

    public string GenerateTemporaryDownloadUrl(string bucketName, string key, TimeSpan expiration)
    {
        try
        {
            var request = new R2GenerateGetSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                ExpiresIn = expiration,
                ResponseContentDisposition = $"attachment; filename=\"{Path.GetFileName(key)}\""
            };

            var response = _r2Client.SignedUrls.GenerateGetSignedUrl(request);
            
            _logger.LogInformation("Generated download URL for {Key} in bucket {BucketName}, expires at {ExpiresAt}",
                key, bucketName, response.ExpiresAt);
            
            return response.SignedUrl;
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to generate download URL for {Key} in bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public string GenerateSecureUploadUrl(string bucketName, string key, string contentType, 
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            var request = new R2GeneratePutSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentType = contentType,
                ExpiresIn = TimeSpan.FromMinutes(30), // Short expiration for uploads
                ServerSideEncryption = "AES256",
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            // Add audit metadata
            request.Metadata["uploaded-via"] = "signed-url";
            request.Metadata["generated-at"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

            var response = _r2Client.SignedUrls.GeneratePutSignedUrl(request);
            
            _logger.LogInformation("Generated upload URL for {Key} in bucket {BucketName}, expires at {ExpiresAt}",
                key, bucketName, response.ExpiresAt);
            
            return response.SignedUrl;
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upload URL for {Key} in bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<bool> ValidateUploadAsync(string signedUrl, HttpClient httpClient)
    {
        try
        {
            // Send a HEAD request to check if the URL is still valid
            var request = new HttpRequestMessage(HttpMethod.Head, signedUrl);
            var response = await httpClient.SendAsync(request);
            
            var isValid = response.IsSuccessStatusCode;
            _logger.LogInformation("Signed URL validation result: {IsValid}", isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate signed URL");
            return false;
        }
    }
}
```