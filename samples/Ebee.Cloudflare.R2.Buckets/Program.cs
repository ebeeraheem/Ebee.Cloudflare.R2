using Ebee.Cloudflare.R2;
using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Buckets.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Replace these with your actual R2 credentials
const string ACCOUNT_ID = "your-account-id-here";
const string ACCESS_KEY_ID = "your-access-key-id-here";
const string SECRET_ACCESS_KEY = "your-secret-access-key-here";

// Test bucket name (will be created and deleted during testing)
const string TEST_BUCKET_NAME = "test-bucket-sample-app";

Console.WriteLine("=== Cloudflare R2 Buckets Client Sample ===\n");

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

// Get the buckets client from DI container
var bucketsClient = host.Services.GetRequiredService<IBucketsClient>();

try
{
    // 1. List existing buckets
    await ListBucketsAsync(bucketsClient);

    // 2. Create a test bucket
    await CreateBucketAsync(bucketsClient, TEST_BUCKET_NAME);

    // 3. List buckets again to show the new bucket
    await ListBucketsAsync(bucketsClient);

    // 4. Attempt to create the same bucket again (should fail)
    await CreateBucketAsync(bucketsClient, TEST_BUCKET_NAME);

    // 5. Delete the test bucket
    await DeleteBucketAsync(bucketsClient, TEST_BUCKET_NAME);

    // 6. List buckets again to confirm deletion
    await ListBucketsAsync(bucketsClient);

    // 7. Attempt to delete a non-existent bucket (should fail)
    await DeleteBucketAsync(bucketsClient, "non-existent-bucket");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}

Console.WriteLine("\n=== Sample completed ===");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static async Task ListBucketsAsync(IBucketsClient bucketsClient)
{
    Console.WriteLine("Listing buckets...");

    try
    {
        var response = await bucketsClient.ListBucketsAsync();

        if (response.Buckets.Count == 0)
        {
            Console.WriteLine("   No buckets found.");
        }
        else
        {
            Console.WriteLine($"   Found {response.Buckets.Count} bucket(s):");
            foreach (var bucket in response.Buckets)
            {
                Console.WriteLine($"   • {bucket.Name} (Created: {bucket.CreationDate:yyyy-MM-dd HH:mm:ss} UTC)");
            }
        }

        if (response.Owner is not null)
        {
            Console.WriteLine($"   Owner: {response.Owner}");
        }

        Console.WriteLine("   List buckets completed successfully.\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to list buckets: {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task CreateBucketAsync(IBucketsClient bucketsClient, string bucketName)
{
    Console.WriteLine($"Creating bucket '{bucketName}'...");

    try
    {
        var request = new R2CreateBucketRequest { BucketName = bucketName };
        var response = await bucketsClient.CreateBucketAsync(request);

        Console.WriteLine($"   Bucket '{response.BucketName}' created successfully!");
        Console.WriteLine($"   Location: {response.Location}");
        Console.WriteLine($"   Creation Date: {response.CreationDate:yyyy-MM-dd HH:mm:ss} UTC\n");
    }
    catch (R2Exception ex)
    {
        Console.WriteLine($"   Failed to create bucket '{bucketName}': {ex.Message}");
        if (ex.InnerException is not null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine();
    }
}

static async Task DeleteBucketAsync(IBucketsClient bucketsClient, string bucketName)
{
    Console.WriteLine($"Deleting bucket '{bucketName}'...");

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
