using Ebee.Cloudflare.R2;
using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Buckets.Models;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.Objects.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

// Replace these with your actual R2 credentials
const string ACCOUNT_ID = "your-account-id-here";
const string ACCESS_KEY_ID = "your-access-key-id-here";
const string SECRET_ACCESS_KEY = "your-secret-access-key-here";

// Test bucket and object names
const string TEST_BUCKET_NAME = "test-bucket-objects-sample";
const string TEST_OBJECT_KEY = "sample-text-file.txt";
const string TEST_OBJECT_KEY_2 = "sample-copy-file.txt";
const string TEST_CONTENT = "Hello, Cloudflare R2! This is a test file created by the ObjectsClient sample.";

Console.WriteLine("=== Cloudflare R2 Objects Client Sample ===\n");

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
var bucketsClient = host.Services.GetRequiredService<IBucketsClient>();
var objectsClient = host.Services.GetRequiredService<IObjectsClient>();

try
{
    // 1. Create test bucket (required for object operations)
    await CreateBucketAsync(bucketsClient, TEST_BUCKET_NAME);

    // 2. List objects in the bucket (should be empty initially)
    await ListObjectsAsync(objectsClient, TEST_BUCKET_NAME);

    // 3. Put a test object
    await PutObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY, TEST_CONTENT);

    // 4. List objects again to show the new object
    await ListObjectsAsync(objectsClient, TEST_BUCKET_NAME);

    // 5. Get object metadata
    await GetObjectMetadataAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 6. Get the object content
    await GetObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 7. Copy the object to a new key
    await CopyObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY, TEST_BUCKET_NAME, TEST_OBJECT_KEY_2);

    // 8. List objects to show both files
    await ListObjectsAsync(objectsClient, TEST_BUCKET_NAME);

    // 9. Delete the original object
    await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);

    // 10. Delete the copied object
    await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY_2);

    // 11. List objects again to confirm deletion
    await ListObjectsAsync(objectsClient, TEST_BUCKET_NAME);

    // 12. Attempt to get a non-existent object (should fail)
    await GetObjectAsync(objectsClient, TEST_BUCKET_NAME, "non-existent-file.txt");

    // 13. Clean up: Delete the test bucket
    await DeleteBucketAsync(bucketsClient, TEST_BUCKET_NAME);
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");

    // Attempt cleanup on error
    try
    {
        Console.WriteLine("Attempting to clean up test bucket...");
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

static async Task ListObjectsAsync(IObjectsClient objectsClient, string bucketName)
{
    Console.WriteLine($"Listing objects in bucket '{bucketName}'...");

    try
    {
        var request = new R2ListObjectsRequest { BucketName = bucketName };
        var response = await objectsClient.ListObjectsAsync(request);

        if (response.Objects.Count == 0)
        {
            Console.WriteLine("   No objects found.");
        }
        else
        {
            Console.WriteLine($"   Found {response.Objects.Count} object(s):");
            foreach (var obj in response.Objects)
            {
                Console.WriteLine($"   • {obj.Key} ({FormatFileSize(obj.Size)}, Modified: {obj.LastModified:yyyy-MM-dd HH:mm:ss} UTC)");
                Console.WriteLine($"     ETag: {obj.ETag}, Storage: {obj.StorageClass}");
                if (!string.IsNullOrEmpty(obj.Owner))
                {
                    Console.WriteLine($"     Owner: {obj.Owner}");
                }
            }
        }

        Console.WriteLine($"   Key Count: {response.KeyCount}, Truncated: {response.IsTruncated}");
        Console.WriteLine("   List objects completed successfully.\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to list objects: {ex.Message}");
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
                ["sample-app"] = "objects-client-demo",
                ["upload-time"] = DateTime.UtcNow.ToString("O")
            }
        };

        var response = await objectsClient.PutObjectAsync(request);

        Console.WriteLine($"   Object '{response.Key}' uploaded successfully!");
        Console.WriteLine($"   ETag: {response.ETag}");
        Console.WriteLine($"   Version ID: {response.VersionId ?? "N/A"}");
        Console.WriteLine($"   Uploaded at: {response.UploadedAt:yyyy-MM-dd HH:mm:ss} UTC");
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

static async Task GetObjectAsync(IObjectsClient objectsClient, string bucketName, string key)
{
    Console.WriteLine($"Downloading object '{key}' from bucket '{bucketName}'...");

    try
    {
        var request = new R2GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        using var response = await objectsClient.GetObjectAsync(request);

        Console.WriteLine($"   Object '{response.Key}' downloaded successfully!");
        Console.WriteLine($"   Content Type: {response.ContentType}");
        Console.WriteLine($"   Content Length: {FormatFileSize(response.ContentLength)}");
        Console.WriteLine($"   ETag: {response.ETag}");
        Console.WriteLine($"   Last Modified: {response.LastModified:yyyy-MM-dd HH:mm:ss} UTC");

        if (response.Metadata.Count > 0)
        {
            Console.WriteLine("   Metadata:");
            foreach (var metadata in response.Metadata)
            {
                Console.WriteLine($"     {metadata.Key}: {metadata.Value}");
            }
        }

        // Display content if it's text
        if (response.ContentType?.StartsWith("text/") == true && response.ContentBytes?.Length < 1000)
        {
            var content = Encoding.UTF8.GetString(response.ContentBytes);
            Console.WriteLine($"   Content Preview: \"{content}\"");
        }
        Console.WriteLine();
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to download object '{key}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task GetObjectMetadataAsync(IObjectsClient objectsClient, string bucketName, string key)
{
    Console.WriteLine($"Getting metadata for object '{key}' in bucket '{bucketName}'...");

    try
    {
        var request = new R2GetObjectMetadataRequest
        {
            BucketName = bucketName,
            Key = key
        };

        var response = await objectsClient.GetObjectMetadataAsync(request);

        Console.WriteLine($"   Metadata retrieved for object '{response.Key}':");
        Console.WriteLine($"   Content Type: {response.ContentType}");
        Console.WriteLine($"   Content Length: {FormatFileSize(response.ContentLength)}");
        Console.WriteLine($"   ETag: {response.ETag}");
        Console.WriteLine($"   Last Modified: {response.LastModified:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"   Storage Class: {response.StorageClass}");

        if (response.Metadata.Count > 0)
        {
            Console.WriteLine("   Custom Metadata:");
            foreach (var metadata in response.Metadata)
            {
                Console.WriteLine($"     {metadata.Key}: {metadata.Value}");
            }
        }
        Console.WriteLine();
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to get metadata for object '{key}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task CopyObjectAsync(
    IObjectsClient objectsClient,
    string sourceBucket,
    string sourceKey,
    string destBucket,
    string destKey)
{
    Console.WriteLine($"Copying object from '{sourceBucket}/{sourceKey}' to '{destBucket}/{destKey}'...");

    try
    {
        var request = new R2CopyObjectRequest
        {
            SourceBucketName = sourceBucket,
            SourceKey = sourceKey,
            DestinationBucketName = destBucket,
            DestinationKey = destKey,
            MetadataDirective = "COPY" // Copy metadata from source
        };

        var response = await objectsClient.CopyObjectAsync(request);

        Console.WriteLine($"   Object copied successfully!");
        Console.WriteLine($"   Destination: {response.DestinationBucketName}/{response.DestinationKey}");
        Console.WriteLine($"   ETag: {response.ETag}");
        Console.WriteLine($"   Version ID: {response.VersionId ?? "N/A"}");
        Console.WriteLine($"   Copied at: {response.CopiedAt:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to copy object: {ex.Message}");
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
