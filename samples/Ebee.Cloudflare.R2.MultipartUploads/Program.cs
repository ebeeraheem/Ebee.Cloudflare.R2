using Ebee.Cloudflare.R2;
using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Buckets.Models;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.Objects.Models;
using Ebee.Cloudflare.R2.MultipartUploads;
using Ebee.Cloudflare.R2.MultipartUploads.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Replace these with your actual R2 credentials
const string ACCOUNT_ID = "your-account-id-here";
const string ACCESS_KEY_ID = "your-access-key-id-here";
const string SECRET_ACCESS_KEY = "your-secret-access-key-here";

// Test bucket and object names
const string TEST_BUCKET_NAME = "test-bucket-multipart-uploads-sample";
const string TEST_OBJECT_KEY = "large-test-file.zip";
const int PART_SIZE = 5 * 1024 * 1024; // 5MB minimum part size

Console.WriteLine("=== Cloudflare R2 Multipart Uploads Client Sample ===\n");

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
var multipartUploadsClient = r2Client.MultipartUploads;

try
{
    // 1. Create test bucket (required for multipart upload operations)
    await CreateBucketAsync(bucketsClient, TEST_BUCKET_NAME);

    // 2. Create test data for multipart upload
    var testData = CreateTestData(PART_SIZE * 3); // Create 15MB of test data (3 parts)

    // 3. Demonstrate complete multipart upload workflow
    await DemonstrateCompleteMultipartUploadAsync(multipartUploadsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY, testData);

    // 4. List multipart uploads (should be empty now)
    await ListMultipartUploadsAsync(multipartUploadsClient, TEST_BUCKET_NAME);

    // 5. Demonstrate multipart upload with manual part management
    await DemonstrateManualMultipartUploadAsync(
        multipartUploadsClient,
        TEST_BUCKET_NAME,
        "manual-upload.zip",
        testData);

    // 6. Demonstrate listing parts of an upload
    var uploadId = await InitiateAndUploadPartsAsync(
        multipartUploadsClient,
        TEST_BUCKET_NAME,
        "parts-demo.zip",
        testData);

    // 7. List parts of the upload
    await ListPartsAsync(multipartUploadsClient, TEST_BUCKET_NAME, "parts-demo.zip", uploadId);

    // 8. Abort the demonstration upload
    await AbortMultipartUploadAsync(multipartUploadsClient, TEST_BUCKET_NAME, "parts-demo.zip", uploadId);

    // 9. Demonstrate error handling scenarios
    await DemonstrateErrorHandlingAsync(multipartUploadsClient, TEST_BUCKET_NAME);

    // 10. Clean up: Delete uploaded objects
    await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);
    await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, "manual-upload.zip");

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

        // Try to delete objects first
        await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, TEST_OBJECT_KEY);
        await DeleteObjectAsync(objectsClient, TEST_BUCKET_NAME, "manual-upload.zip");

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

static byte[] CreateTestData(int totalSize)
{
    Console.WriteLine($"Creating test data ({FormatFileSize(totalSize)})...");

    var data = new byte[totalSize];
    var random = new Random();

    // Fill with random data to simulate a real file
    random.NextBytes(data);

    // Add some recognizable pattern at the beginning
    var header = "MULTIPART-UPLOAD-TEST-DATA"u8.ToArray();
    Array.Copy(header, data, Math.Min(header.Length, data.Length));

    Console.WriteLine($"   Test data created successfully ({FormatFileSize(data.Length)})\n");
    return data;
}

static async Task DemonstrateCompleteMultipartUploadAsync(
    IMultipartUploadsClient multipartClient,
    string bucketName,
    string key,
    byte[] data)
{
    Console.WriteLine($"Demonstrating complete multipart upload workflow for '{key}'...");

    try
    {
        // Step 1: Initiate multipart upload
        var initiateRequest = new R2InitiateMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentType = "application/zip",
            ServerSideEncryption = "AES256",
            Metadata = new Dictionary<string, string>
            {
                { "upload-method", "multipart" },
                { "sample-app", "multipart-uploads-demo" },
                { "created-at", DateTime.UtcNow.ToString("O") }
            }
        };

        var initiateResponse = await multipartClient.InitiateMultipartUploadAsync(initiateRequest);
        var uploadId = initiateResponse.UploadId;

        Console.WriteLine($"   Multipart upload initiated successfully!");
        Console.WriteLine($"   Upload ID: {uploadId}");
        Console.WriteLine($"   Server-side encryption: {initiateResponse.ServerSideEncryption}");
        Console.WriteLine($"   Initiated at: {initiateResponse.InitiatedAt:yyyy-MM-dd HH:mm:ss} UTC");

        // Step 2: Upload parts
        var parts = SplitDataIntoParts(data, PART_SIZE);
        var completedParts = new List<R2CompletedPart>();

        Console.WriteLine($"   Uploading {parts.Count} parts...");

        for (int i = 0; i < parts.Count; i++)
        {
            var partNumber = i + 1;
            var partData = parts[i];

            var uploadPartRequest = new R2UploadPartRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                PartNumber = partNumber,
                ContentBytes = partData
            };

            var partResponse = await multipartClient.UploadPartAsync(uploadPartRequest);

            completedParts.Add(new R2CompletedPart
            {
                PartNumber = partResponse.PartNumber,
                ETag = partResponse.ETag
            });

            Console.WriteLine($"     Part {partNumber}: {FormatFileSize(partData.Length)}, ETag: {partResponse.ETag}");
        }

        // Step 3: Complete multipart upload
        var completeRequest = new R2CompleteMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            UploadId = uploadId,
            Parts = completedParts
        };

        var completeResponse = await multipartClient.CompleteMultipartUploadAsync(completeRequest);

        Console.WriteLine($"   Multipart upload completed successfully!");
        Console.WriteLine($"   Final ETag: {completeResponse.ETag}");
        Console.WriteLine($"   Location: {completeResponse.Location}");
        Console.WriteLine($"   Version ID: {completeResponse.VersionId ?? "N/A"}");
        Console.WriteLine($"   Server-side encryption: {completeResponse.ServerSideEncryption}");
        Console.WriteLine($"   Completed at: {completeResponse.CompletedAt:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to complete multipart upload: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task DemonstrateManualMultipartUploadAsync(
    IMultipartUploadsClient multipartClient,
    string bucketName,
    string key,
    byte[] data)
{
    Console.WriteLine($"Demonstrating manual multipart upload management for '{key}'...");

    string uploadId = string.Empty;

    try
    {
        // Initiate upload with advanced options
        var initiateRequest = new R2InitiateMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentType = "application/zip",
            CacheControl = "max-age=3600",
            ContentDisposition = "attachment; filename=\"manual-upload.zip\"",
            ContentEncoding = "gzip",
            StorageClass = "STANDARD",
            Metadata = new Dictionary<string, string>
            {
                { "upload-type", "manual-demo" },
                { "part-size", PART_SIZE.ToString() },
                { "total-size", data.Length.ToString() }
            }
        };

        var initiateResponse = await multipartClient.InitiateMultipartUploadAsync(initiateRequest);
        uploadId = initiateResponse.UploadId;

        Console.WriteLine($"   Upload initiated with ID: {uploadId}");

        // Upload parts with different content sources
        var parts = SplitDataIntoParts(data, PART_SIZE);
        var completedParts = new List<R2CompletedPart>();

        for (int i = 0; i < parts.Count; i++)
        {
            var partNumber = i + 1;
            var partData = parts[i];

            R2UploadPartRequest uploadPartRequest;

            // Demonstrate different content sources
            if (i == 0)
            {
                // First part: Use byte array
                uploadPartRequest = new R2UploadPartRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    ContentBytes = partData
                };
                Console.WriteLine($"     Part {partNumber}: Using ContentBytes ({FormatFileSize(partData.Length)})");
            }
            else
            {
                // Other parts: Use stream
                var stream = new MemoryStream(partData);
                uploadPartRequest = new R2UploadPartRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    ContentStream = stream
                };
                Console.WriteLine($"     Part {partNumber}: Using ContentStream ({FormatFileSize(partData.Length)})");
            }

            var partResponse = await multipartClient.UploadPartAsync(uploadPartRequest);

            completedParts.Add(new R2CompletedPart
            {
                PartNumber = partResponse.PartNumber,
                ETag = partResponse.ETag
            });

            Console.WriteLine($"       Uploaded successfully, ETag: {partResponse.ETag}");

            // Dispose stream if used
            if (uploadPartRequest.ContentStream is not null)
            {
                await uploadPartRequest.ContentStream.DisposeAsync();
            }
        }

        // Complete upload
        var completeRequest = new R2CompleteMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            UploadId = uploadId,
            Parts = completedParts
        };

        var completeResponse = await multipartClient.CompleteMultipartUploadAsync(completeRequest);

        Console.WriteLine($"   Manual multipart upload completed!");
        Console.WriteLine($"   Final ETag: {completeResponse.ETag}");
        Console.WriteLine($"   Location: {completeResponse.Location}\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed manual multipart upload: {ex.Message}");

        // Cleanup: Try to abort the upload
        if (!string.IsNullOrEmpty(uploadId))
        {
            try
            {
                await AbortMultipartUploadAsync(multipartClient, bucketName, key, uploadId);
                Console.WriteLine($"   Aborted failed upload {uploadId}");
            }
            catch
            {
                Console.WriteLine($"   Could not abort upload {uploadId}");
            }
        }

        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task<string> InitiateAndUploadPartsAsync(
    IMultipartUploadsClient multipartClient,
    string bucketName,
    string key,
    byte[] data)
{
    Console.WriteLine($"Initiating upload for parts demonstration '{key}'...");

    try
    {
        // Initiate upload
        var initiateRequest = new R2InitiateMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentType = "application/zip",
            Metadata = new Dictionary<string, string>
            {
                { "purpose", "parts-demonstration" }
            }
        };

        var initiateResponse = await multipartClient.InitiateMultipartUploadAsync(initiateRequest);
        var uploadId = initiateResponse.UploadId;

        Console.WriteLine($"   Upload ID: {uploadId}");

        // Upload first two parts only (for demonstration)
        var parts = SplitDataIntoParts(data, PART_SIZE);
        var partsToUpload = Math.Min(2, parts.Count);

        Console.WriteLine($"   Uploading {partsToUpload} parts for demonstration...");

        for (int i = 0; i < partsToUpload; i++)
        {
            var partNumber = i + 1;
            var partData = parts[i];

            var uploadPartRequest = new R2UploadPartRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                PartNumber = partNumber,
                ContentBytes = partData
            };

            var partResponse = await multipartClient.UploadPartAsync(uploadPartRequest);
            Console.WriteLine($"     Part {partNumber} uploaded, ETag: {partResponse.ETag}");
        }

        Console.WriteLine();
        return uploadId;
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to initiate and upload parts: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
        return string.Empty;
    }
}

static async Task ListMultipartUploadsAsync(IMultipartUploadsClient multipartClient, string bucketName)
{
    Console.WriteLine($"Listing multipart uploads in bucket '{bucketName}'...");

    try
    {
        var request = new R2ListMultipartUploadsRequest
        {
            BucketName = bucketName,
            MaxUploads = 10
        };

        var response = await multipartClient.ListMultipartUploadsAsync(request);

        if (response.Uploads.Count == 0)
        {
            Console.WriteLine("   No ongoing multipart uploads found.");
        }
        else
        {
            Console.WriteLine($"   Found {response.Uploads.Count} ongoing upload(s):");
            foreach (var upload in response.Uploads)
            {
                Console.WriteLine($"   • Key: {upload.Key}");
                Console.WriteLine($"     Upload ID: {upload.UploadId}");
                Console.WriteLine($"     Initiated: {upload.Initiated:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"     Storage Class: {upload.StorageClass}");
                Console.WriteLine($"     Owner: {upload.Owner ?? "N/A"}");
                Console.WriteLine("     ---");
            }
        }

        Console.WriteLine($"   Max uploads: {response.MaxUploads}, Truncated: {response.IsTruncated}");
        Console.WriteLine("   List multipart uploads completed successfully.\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to list multipart uploads: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task ListPartsAsync(IMultipartUploadsClient multipartClient, string bucketName, string key, string uploadId)
{
    Console.WriteLine($"Listing parts for upload '{uploadId}' of object '{key}'...");

    try
    {
        var request = new R2ListPartsRequest
        {
            BucketName = bucketName,
            Key = key,
            UploadId = uploadId,
            MaxParts = 10
        };

        var response = await multipartClient.ListPartsAsync(request);

        if (response.Parts.Count == 0)
        {
            Console.WriteLine("   No parts found for this upload.");
        }
        else
        {
            Console.WriteLine($"   Found {response.Parts.Count} part(s):");
            foreach (var part in response.Parts)
            {
                Console.WriteLine($"   • Part {part.PartNumber}: {FormatFileSize(part.Size)} bytes");
                Console.WriteLine($"     ETag: {part.ETag}");
                Console.WriteLine($"     Last Modified: {part.LastModified:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine("     ---");
            }
        }

        Console.WriteLine($"   Storage Class: {response.StorageClass}");
        Console.WriteLine($"   Owner: {response.Owner ?? "N/A"}");
        Console.WriteLine($"   Max parts: {response.MaxParts}, Truncated: {response.IsTruncated}");
        Console.WriteLine("   List parts completed successfully.\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to list parts: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task AbortMultipartUploadAsync(
    IMultipartUploadsClient multipartClient,
    string bucketName,
    string key,
    string uploadId)
{
    Console.WriteLine($"Aborting multipart upload '{uploadId}' for object '{key}'...");

    try
    {
        var request = new R2AbortMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            UploadId = uploadId
        };

        var response = await multipartClient.AbortMultipartUploadAsync(request);

        Console.WriteLine($"   Multipart upload aborted successfully!");
        Console.WriteLine($"   Upload ID: {response.UploadId}");
        Console.WriteLine($"   Object: {response.Key}");
        Console.WriteLine($"   Aborted at: {response.AbortedAt:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to abort multipart upload: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task DemonstrateErrorHandlingAsync(IMultipartUploadsClient multipartClient, string bucketName)
{
    Console.WriteLine("Demonstrating error handling scenarios...");

    // Test 1: Invalid part number
    try
    {
        var invalidRequest = new R2UploadPartRequest
        {
            BucketName = bucketName,
            Key = "test-file.txt",
            UploadId = "fake-upload-id",
            PartNumber = 0, // Invalid part number
            ContentBytes = "test"u8.ToArray()
        };

        _ = await multipartClient.UploadPartAsync(invalidRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for invalid part number");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine($"   Correctly caught invalid part number error: {ex.GetType().Name}");
    }

    // Test 2: No content source
    try
    {
        var noContentRequest = new R2UploadPartRequest
        {
            BucketName = bucketName,
            Key = "test-file.txt",
            UploadId = "fake-upload-id",
            PartNumber = 1
            // No content source provided
        };

        _ = await multipartClient.UploadPartAsync(noContentRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for no content source");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"   Correctly caught no content source error: {ex.GetType().Name}");
    }

    // Test 3: Non-existent upload ID
    try
    {
        var nonExistentRequest = new R2UploadPartRequest
        {
            BucketName = bucketName,
            Key = "test-file.txt",
            UploadId = "non-existent-upload-id",
            PartNumber = 1,
            ContentBytes = "test"u8.ToArray()
        };

        _ = await multipartClient.UploadPartAsync(nonExistentRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for non-existent upload");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Correctly caught non-existent upload error: {ex.Message}");
    }

    // Test 4: Empty bucket name
    try
    {
        var emptyBucketRequest = new R2InitiateMultipartUploadRequest
        {
            BucketName = string.Empty,
            Key = "test-file.txt"
        };

        _ = await multipartClient.InitiateMultipartUploadAsync(emptyBucketRequest);
        Console.WriteLine("   ERROR: Should have thrown exception for empty bucket name");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"   Correctly caught empty bucket name error: {ex.GetType().Name}");
    }

    Console.WriteLine("   Error handling demonstration completed.\n");
}

static List<byte[]> SplitDataIntoParts(byte[] data, int partSize)
{
    var parts = new List<byte[]>();

    for (int i = 0; i < data.Length; i += partSize)
    {
        var currentPartSize = Math.Min(partSize, data.Length - i);
        var part = new byte[currentPartSize];
        Array.Copy(data, i, part, 0, currentPartSize);
        parts.Add(part);
    }

    return parts;
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
