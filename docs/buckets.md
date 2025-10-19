# Buckets

The R2 client provides methods for managing R2 buckets, including listing, creating, and deleting buckets.

## Getting Started

The R2 client is automatically registered when you configure it in your dependency injection container:

```csharp
services.AddR2Client(options =>
{
    options.AccessKeyId = "your-access-key";
    options.SecretAccessKey = "your-secret-key";
    options.AccountId = "your-account-id";
});
```

Then inject `IR2Client` and use the bucket methods directly:

```csharp
public class MyService
{
    private readonly IR2Client _r2Client;

    public MyService(IR2Client r2Client)
    {
        _r2Client = r2Client;
    }

    public async Task ManageBuckets()
    {
        // Use bucket methods directly on the client
        var response = await _r2Client.ListBucketsAsync();
        // ...
    }
}
```

## Listing Buckets

Retrieve a list of all buckets in your R2 account:

```csharp
var response = await _r2Client.ListBucketsAsync();

Console.WriteLine($"Owner: {response.Owner}");
foreach (var bucket in response.Buckets)
{
    Console.WriteLine($"Bucket: {bucket.Name}, Created: {bucket.CreationDate}");
}
```

### Response

The `ListBucketsAsync` method returns an `R2ListBucketsResponse` containing:

- `Buckets`: List of `R2BucketInfoResponse` objects with bucket details
- `Owner`: The display name of the bucket owner

## Creating Buckets

Create a new bucket in your R2 account:

```csharp
var request = new R2CreateBucketRequest
{
    BucketName = "my-new-bucket"
};

var response = await _r2Client.CreateBucketAsync(request);

Console.WriteLine($"Created bucket: {response.BucketName}");
Console.WriteLine($"Location: {response.Location}");
Console.WriteLine($"Creation Date: {response.CreationDate}");
```

### Bucket Naming Rules

Bucket names must follow these rules:
- Must be globally unique across all R2 accounts
- Must be between 3-63 characters long
- Can contain lowercase letters, numbers, and hyphens
- Must start and end with a letter or number
- Cannot contain uppercase letters or underscores

### Error Handling

The create operation may throw `R2Exception` with specific error messages:

- **BucketAlreadyExists**: The bucket name is already taken by another account
- **BucketAlreadyOwnedByYou**: You already own a bucket with this name
- **InvalidBucketName**: The bucket name doesn't meet naming requirements

```csharp
try
{
    var response = await _r2Client.CreateBucketAsync(request);
    Console.WriteLine($"Bucket created successfully: {response.BucketName}");
}
catch (R2Exception ex)
{
    Console.WriteLine($"Failed to create bucket: {ex.Message}");
}
```

## Deleting Buckets

Delete an existing bucket from your R2 account:

```csharp
var request = new R2DeleteBucketRequest
{
    BucketName = "bucket-to-delete"
};

var response = await _r2Client.DeleteBucketAsync(request);
Console.WriteLine($"Deleted bucket: {response.BucketName}");
```

### Important Notes

- The bucket must be empty before it can be deleted
- This operation is irreversible
- All objects must be deleted from the bucket first

### Error Handling

The delete operation may throw `R2Exception` with specific error messages:

- **NoSuchBucket**: The specified bucket doesn't exist
- **BucketNotEmpty**: The bucket contains objects and cannot be deleted

```csharp
try
{
    var response = await _r2Client.DeleteBucketAsync(request);
    Console.WriteLine($"Bucket deleted successfully: {response.BucketName}");
}
catch (R2Exception ex)
{
    Console.WriteLine($"Failed to delete bucket: {ex.Message}");
}
```

## API Reference

### IR2Client Interface (Bucket Methods)

```csharp
public interface IR2Client
{
    Task<R2ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default);
    Task<R2CreateBucketResponse> CreateBucketAsync(R2CreateBucketRequest request, CancellationToken cancellationToken = default);
    Task<R2DeleteBucketResponse> DeleteBucketAsync(R2DeleteBucketRequest request, CancellationToken cancellationToken = default);
}
```

### Request Models

#### R2CreateBucketRequest
```csharp
public class R2CreateBucketRequest
{
    public required string BucketName { get; set; }
}
```

#### R2DeleteBucketRequest
```csharp
public class R2DeleteBucketRequest
{
    public required string BucketName { get; set; }
}
```

### Response Models

#### R2ListBucketsResponse
```csharp
public class R2ListBucketsResponse
{
    public List<R2BucketInfoResponse> Buckets { get; set; } = [];
    public string? Owner { get; set; }
}
```

#### R2BucketInfoResponse
```csharp
public class R2BucketInfoResponse
{
    public required string Name { get; set; }
    public DateTime? CreationDate { get; set; }
}
```

#### R2CreateBucketResponse
```csharp
public class R2CreateBucketResponse
{
    public required string BucketName { get; set; }
    public string? Location { get; set; }
    public DateTime CreationDate { get; set; }
}
```

#### R2DeleteBucketResponse
```csharp
public class R2DeleteBucketResponse
{
    public required string BucketName { get; set; }
}
```

## Best Practices

1. **Bucket Naming**: Choose descriptive, lowercase names that clearly indicate the bucket's purpose
2. **Error Handling**: Always wrap bucket operations in try-catch blocks to handle R2 exceptions gracefully
3. **Cleanup**: Before deleting a bucket, ensure all objects are removed first
4. **Monitoring**: Log bucket operations for auditing and debugging purposes

## Example: Complete Bucket Management

```csharp
public class BucketManager
{
    private readonly IR2Client _r2Client;
    private readonly ILogger<BucketManager> _logger;

    public BucketManager(IR2Client r2Client, ILogger<BucketManager> logger)
    {
        _r2Client = r2Client;
        _logger = logger;
    }

    public async Task<bool> CreateBucketIfNotExistsAsync(string bucketName)
    {
        try
        {
            var createRequest = new R2CreateBucketRequest { BucketName = bucketName };
            var response = await _r2Client.CreateBucketAsync(createRequest);
            
            _logger.LogInformation("Created bucket {BucketName} at {CreationDate}", 
                response.BucketName, response.CreationDate);
            return true;
        }
        catch (R2Exception ex) when (ex.Message.Contains("already exists") || 
                                     ex.Message.Contains("already owned"))
        {
            _logger.LogInformation("Bucket {BucketName} already exists", bucketName);
            return false;
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to create bucket {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<List<string>> GetAllBucketNamesAsync()
    {
        try
        {
            var response = await _r2Client.ListBucketsAsync();
            return response.Buckets.Select(b => b.Name).ToList();
        }
        catch (R2Exception ex)
        {
            _logger.LogError(ex, "Failed to list buckets");
            throw;
        }
    }
}
```