using Ebee.Cloudflare.R2;
using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Buckets.Models;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.Objects.Models;
using Ebee.Cloudflare.R2.SignedUrls;
using Ebee.Cloudflare.R2.SignedUrls.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

// Replace these with your actual R2 credentials
const string ACCOUNT_ID = "your-account-id-here";
const string ACCESS_KEY_ID = "your-access-key-id-here";
const string SECRET_ACCESS_KEY = "your-secret-access-key-here";

// Test bucket and object names
const string TEST_BUCKET_NAME = "test-bucket-signed-urls-sample";
const string TEST_OBJECT_KEY = "sample-signed-url-file.txt";
const string TEST_CONTENT = "Hello, Cloudflare R2! This is a test file for signed URLs demonstration.";

Console.WriteLine("=== Cloudflare R2 Signed URLs Client Sample ===\n");

// Validate credentials are set
ArgumentException.ThrowIfNullOrEmpty(ACCOUNT_ID);
ArgumentException.ThrowIfNullOrEmpty(ACCESS_KEY_ID);
ArgumentException.ThrowIfNullOrEmpty(SECRET_ACCESS_KEY);

// Setup dependency injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddR2Client(options =>
        {
            options.AccountId = ACCOUNT_ID;
            options.AccessKeyId = ACCESS_KEY_ID;
            options.SecretAccessKey = SECRET_ACCESS_KEY;
        });
    })
    .Build();

// Get clients from DI container
var r2Client = host.Services.GetRequiredService<IR2Client>();
var bucketsClient = r2Client.Buckets;
var objectsClient = r2Client.Objects;
var signedUrlsClient = r2Client.SignedUrls;

try
{
    // 1. Create test bucket (required for signed URL operations)
    await CreateBucketAsync(bucketsClient, TEST_BUCKET_NAME);

    // 2. Upload a test object
    await PutObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY, TEST_CONTENT);

    // 3. Generate GET signed URL with basic options
    await GenerateGetSignedUrlAsync(signedUrlsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 4. Generate GET signed URL with response headers
    await GenerateGetSignedUrlWithHeadersAsync(signedUrlsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 5. Generate PUT signed URL for uploads
    await GeneratePutSignedUrlAsync(signedUrlsClient, TEST_BUCKET_NAME, "upload-test.txt");

    // 6. Generate PUT signed URL with metadata
    await GeneratePutSignedUrlWithMetadataAsync(signedUrlsClient, TEST_BUCKET_NAME, "upload-with-metadata.txt");

    // 7. Generate DELETE signed URL
    await GenerateDeleteSignedUrlAsync(signedUrlsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 8. Demonstrate expiration options
    await DemonstrateExpirationOptionsAsync(signedUrlsClient, TEST_BUCKET_NAME, "expiration-test.txt");

    // 9. Demonstrate error handling with invalid parameters
    await DemonstrateErrorHandlingAsync(signedUrlsClient);

    // 10. Clean up: Delete the test object before deleting the bucket
    await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 11. Clean up: Delete the test bucket
    await DeleteBucketAsync(bucketsClient, TEST_BUCKET_NAME);
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");

    // Attempt cleanup on error
    try
    {
        Console.WriteLine("Attempting to clean up test bucket...");

        // Try to delete the object first
        await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

        // Then delete the bucket
        await DeleteBucketAsync(bucketsClient, TEST_BUCKET_NAME);
    }
    catch
    {
        Console.WriteLine("Could not clean up test bucket. You may need to delete it manually.");
    }
}

Console.WriteLine("\n=== Sample completed ===");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static async Task CreateBucketAsync(IBucketsClient bucketsClient, string bucketName)
{
    Console.WriteLine($"Creating test bucket '{bucketName}'...");

    try
    {
        var request = new R2CreateBucketRequest { BucketName = bucketName };
        var response = await bucketsClient.CreateBucketAsync(request);

        Console.WriteLine($"   Bucket '{response.BucketName}' created successfully!");
        Console.WriteLine($"   Location: {response.Location}");
        Console.WriteLine($"   Creation Date: {response.CreationDate:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("already owned"))
    {
        Console.WriteLine($"   Bucket '{bucketName}' already exists, continuing with sample...\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to create bucket '{bucketName}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
        throw; // Re-throw to stop execution
    }
}

static async Task DeleteBucketAsync(IBucketsClient bucketsClient, string bucketName)
{
    Console.WriteLine($"Deleting test bucket '{bucketName}'...");

    try
    {
        var request = new R2DeleteBucketRequest { BucketName = bucketName };
        var response = await bucketsClient.DeleteBucketAsync(request);

        Console.WriteLine($"   Bucket '{response.BucketName}' deleted successfully!\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to delete bucket '{bucketName}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task PutObjectAsync(IObjectsClient objectsClient, string bucketName, string key, string content)
{
    Console.WriteLine($"Uploading object '{key}' to bucket '{bucketName}'...");

    try
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var request = new R2PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentBytes = contentBytes,
            ContentType = "text/plain",
            Metadata = new Dictionary<string, string>
            {
                ["sample-app"] = "signed-urls-demo",
                ["upload-time"] = DateTime.UtcNow.ToString("O")
            }
        };

        var response = await objectsClient.PutObjectAsync(request);

        Console.WriteLine($"   Object '{response.Key}' uploaded successfully!");
        Console.WriteLine($"   ETag: {response.ETag}");
        Console.WriteLine($"   Size: {FormatFileSize(contentBytes.Length)}\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to upload object '{key}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task DeleteObjectAsync(IObjectsClient objectsClient, string bucketName, string key)
{
    Console.WriteLine($"Deleting object '{key}' from bucket '{bucketName}'...");

    try
    {
        var request = new R2DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        var response = await objectsClient.DeleteObjectAsync(request);

        Console.WriteLine($"   Object '{response.Key}' deleted successfully!");
        Console.WriteLine($"   Version ID: {response.VersionId ?? "N/A"}");
        Console.WriteLine($"   Delete Marker: {response.DeleteMarker}");
        Console.WriteLine($"   Deleted at: {response.DeletedAt:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to delete object '{key}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task GenerateGetSignedUrlAsync(ISignedUrlsClient signedUrlsClient, string bucketName, string key)
{
    Console.WriteLine($"Generating GET signed URL for '{key}' in bucket '{bucketName}'...");

    try
    {
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ExpiresIn = TimeSpan.FromHours(2)
        };

        var response = signedUrlsClient.GenerateGetSignedUrl(request);

        Console.WriteLine($"   Signed URL generated successfully!");
        Console.WriteLine($"   URL: {response.SignedUrl}");
        Console.WriteLine($"   HTTP Method: {response.HttpMethod}");
        Console.WriteLine($"   Expires at: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"   Generated at: {response.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to generate GET signed URL: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }

    await Task.CompletedTask;
}

static async Task GenerateGetSignedUrlWithHeadersAsync(ISignedUrlsClient signedUrlsClient, string bucketName, string key)
{
    Console.WriteLine($"Generating GET signed URL with response headers for '{key}'...");

    try
    {
        var request = new R2GenerateGetSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ExpiresIn = TimeSpan.FromMinutes(30),
            ResponseContentType = "text/plain",
            ResponseContentDisposition = "attachment; filename=\"downloaded-file.txt\"",
            ResponseCacheControl = "max-age=3600, must-revalidate",
            ResponseExpires = DateTime.UtcNow.AddHours(1)
        };

        var response = signedUrlsClient.GenerateGetSignedUrl(request);

        Console.WriteLine($"   GET signed URL with headers generated successfully!");
        Console.WriteLine($"   URL: {response.SignedUrl}");
        Console.WriteLine($"   Expires at: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"   Response will include custom headers for content type, disposition, and caching\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to generate GET signed URL with headers: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }

    await Task.CompletedTask;
}

static async Task GeneratePutSignedUrlAsync(ISignedUrlsClient signedUrlsClient, string bucketName, string key)
{
    Console.WriteLine($"Generating PUT signed URL for uploading '{key}'...");

    try
    {
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentType = "text/plain",
            ExpiresIn = TimeSpan.FromMinutes(15)
        };

        var response = signedUrlsClient.GeneratePutSignedUrl(request);

        Console.WriteLine($"   PUT signed URL generated successfully!");
        Console.WriteLine($"   URL: {response.SignedUrl}");
        Console.WriteLine($"   HTTP Method: {response.HttpMethod}");
        Console.WriteLine($"   Expires at: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"   Clients can upload directly to this URL for the next {request.ExpiresIn?.TotalMinutes} minutes\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to generate PUT signed URL: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }

    await Task.CompletedTask;
}

static async Task GeneratePutSignedUrlWithMetadataAsync(ISignedUrlsClient signedUrlsClient, string bucketName, string key)
{
    Console.WriteLine($"Generating PUT signed URL with metadata for '{key}'...");

    try
    {
        var request = new R2GeneratePutSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentType = "application/pdf",
            CacheControl = "max-age=86400",
            ContentDisposition = "inline; filename=\"document.pdf\"",
            ContentEncoding = "gzip",
            ServerSideEncryption = "AES256",
            StorageClass = "STANDARD",
            ExpiresIn = TimeSpan.FromHours(1),
            Metadata = new Dictionary<string, string>
            {
                { "author", "John Doe" },
                { "department", "Engineering" },
                { "document-type", "specification" },
                { "version", "1.0" },
                { "upload-date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
            }
        };

        var response = signedUrlsClient.GeneratePutSignedUrl(request);

        Console.WriteLine($"   PUT signed URL with metadata generated successfully!");
        Console.WriteLine($"   URL: {response.SignedUrl}");
        Console.WriteLine($"   Includes custom metadata: {request.Metadata.Count} fields");
        Console.WriteLine($"   Server-side encryption: {request.ServerSideEncryption}");
        Console.WriteLine($"   Storage class: {request.StorageClass}");
        Console.WriteLine($"   Expires at: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to generate PUT signed URL with metadata: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }

    await Task.CompletedTask;
}

static async Task GenerateDeleteSignedUrlAsync(ISignedUrlsClient signedUrlsClient, string bucketName, string key)
{
    Console.WriteLine($"Generating DELETE signed URL for '{key}'...");

    try
    {
        var request = new R2GenerateDeleteSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ExpiresIn = TimeSpan.FromMinutes(10)
        };

        var response = signedUrlsClient.GenerateDeleteSignedUrl(request);

        Console.WriteLine($"   DELETE signed URL generated successfully!");
        Console.WriteLine($"   URL: {response.SignedUrl}");
        Console.WriteLine($"   HTTP Method: {response.HttpMethod}");
        Console.WriteLine($"   Expires at: {response.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"   Clients can delete the object using this URL for the next {request.ExpiresIn?.TotalMinutes} minutes\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to generate DELETE signed URL: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }

    await Task.CompletedTask;
}

static async Task DemonstrateExpirationOptionsAsync(ISignedUrlsClient signedUrlsClient, string bucketName, string key)
{
    Console.WriteLine("Demonstrating different expiration options...");

    try
    {
        // Option 1: Using ExpiresIn (duration from now)
        var request1 = new R2GenerateGetSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ExpiresIn = TimeSpan.FromMinutes(30)
        };

        var response1 = signedUrlsClient.GenerateGetSignedUrl(request1);
        Console.WriteLine($"   ExpiresIn option: URL expires at {response1.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");

        // Option 2: Using explicit Expires (absolute time)
        var explicitExpires = DateTime.UtcNow.AddHours(6);
        var request2 = new R2GenerateGetSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = explicitExpires
        };

        var response2 = signedUrlsClient.GenerateGetSignedUrl(request2);
        Console.WriteLine($"   Explicit Expires option: URL expires at {response2.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");

        // Option 3: Using default expiration (1 hour)
        var request3 = new R2GenerateGetSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key
            // No expiration specified - defaults to 1 hour
        };

        var response3 = signedUrlsClient.GenerateGetSignedUrl(request3);
        Console.WriteLine($"   Default expiration: URL expires at {response3.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC (1 hour from now)");

        // Option 4: Maximum allowed expiration (7 days)
        var request4 = new R2GenerateGetSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            ExpiresIn = TimeSpan.FromDays(7)
        };

        var response4 = signedUrlsClient.GenerateGetSignedUrl(request4);
        Console.WriteLine($"   Maximum expiration: URL expires at {response4.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC (7 days from now)\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to demonstrate expiration options: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }

    await Task.CompletedTask;
}

static async Task DemonstrateErrorHandlingAsync(ISignedUrlsClient signedUrlsClient)
{
    Console.WriteLine("Demonstrating error handling scenarios...");

    // Test 1: Invalid expiration (too long)
    try
    {
        var invalidRequest = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-file.txt",
            ExpiresIn = TimeSpan.FromDays(8) // Too long - exceeds 7 days
        };

        _ = signedUrlsClient.GenerateGetSignedUrl(invalidRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for invalid expiration");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Correctly caught invalid expiration error: {ex.Message}");
    }

    // Test 2: Negative expiration
    try
    {
        var negativeRequest = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = "test-file.txt",
            ExpiresIn = TimeSpan.FromHours(-1) // Negative duration
        };

        _ = signedUrlsClient.GenerateGetSignedUrl(negativeRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for negative expiration");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Correctly caught negative expiration error: {ex.Message}");
    }

    // Test 3: Empty bucket name
    try
    {
        var emptyBucketRequest = new R2GenerateGetSignedUrlRequest
        {
            BucketName = string.Empty,
            Key = "test-file.txt"
        };

        _ = signedUrlsClient.GenerateGetSignedUrl(emptyBucketRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for empty bucket name");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"   Correctly caught empty bucket name error: {ex.GetType().Name}");
    }

    // Test 4: Empty key
    try
    {
        var emptyKeyRequest = new R2GenerateGetSignedUrlRequest
        {
            BucketName = "test-bucket",
            Key = string.Empty
        };

        _ = signedUrlsClient.GenerateGetSignedUrl(emptyKeyRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for empty key");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"   Correctly caught empty key error: {ex.GetType().Name}");
    }

    Console.WriteLine("   Error handling demonstration completed.\n");

    await Task.CompletedTask;
}

static string FormatFileSize(long? bytes)
{
    string[] sizes = ["B", "KB", "MB", "GB", "TB"];
    double len = bytes ?? 0;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
        order++;
        len /= 1024;
    }
    return $"{len:0.##} {sizes[order]}";
}
