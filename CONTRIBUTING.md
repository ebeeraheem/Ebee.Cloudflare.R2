# Contributing to Ebee.Cloudflare.R2

Thank you for your interest in contributing to the Ebee.Cloudflare.R2 library! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Environment](#development-environment)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Submitting Changes](#submitting-changes)
- [Documentation](#documentation)
- [Release Process](#release-process)

## Code of Conduct

This project adheres to a code of conduct that we expect all contributors to follow. Please be respectful and constructive in all interactions.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- Git
- Valid Cloudflare R2 credentials (for integration testing)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/your-username/Ebee.Cloudflare.R2.git
   cd Ebee.Cloudflare.R2
   ```

3. Add the upstream remote:
   ```bash
   git remote add upstream https://github.com/ebeeraheem/Ebee.Cloudflare.R2.git
   ```

## Development Environment

### Building the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running Samples

1. Navigate to any sample project (e.g., `samples/Ebee.Cloudflare.R2.Objects`)
2. Update the credentials in `Program.cs`:
   ```csharp
   const string ACCOUNT_ID = "your-account-id-here";
   const string ACCESS_KEY_ID = "your-access-key-id-here";
   const string SECRET_ACCESS_KEY = "your-secret-access-key-here";
   ```
3. Run the sample:
   ```bash
   dotnet run
   ```

## Project Structure

```
├── src/
│   └── Ebee.Cloudflare.R2/           # Main library
│       ├── Buckets/                  # Bucket operations
│       ├── Objects/                  # Object operations
│       ├── SignedUrls/               # Signed URL operations
│       └── MultipartUploads/         # Multipart upload operations
├── tests/
│   └── Ebee.Cloudflare.R2.Tests/    # Unit tests
├── samples/
│   ├── Ebee.Cloudflare.R2.Buckets/  # Bucket samples
│   ├── Ebee.Cloudflare.R2.Objects/  # Object samples
│   ├── Ebee.Cloudflare.R2.SignedUrls/ # Signed URL samples
│   └── Ebee.Cloudflare.R2.MultipartUploads/ # Multipart samples
└── docs/                             # Documentation
```

## Coding Standards

### C# Conventions

We follow Microsoft's C# coding conventions with these specific guidelines:

#### General Style
```csharp
// ✅ File-scoped namespace (C# 10+)
namespace Ebee.Cloudflare.R2.Objects;

// ✅ Primary constructor (C# 12+)
public class ObjectsClient(IAmazonS3 s3Client) : IObjectsClient
{
    private readonly IAmazonS3 _s3Client = s3Client 
        ?? throw new ArgumentNullException(nameof(s3Client));
}

// ✅ Required properties
public class R2PutObjectRequest
{
    public required string BucketName { get; set; }
    public required string Key { get; set; }
}
```

#### Null Handling
```csharp
// ✅ Use null checks with patterns
if (request is null)
    throw new ArgumentNullException(nameof(request));

// ✅ Use ArgumentNullException.ThrowIfNull (C# 11+)
ArgumentNullException.ThrowIfNull(request);

// ✅ Use null-coalescing operators
var result = value ?? defaultValue;
property ??= new List<string>();
```

#### Exception Handling
```csharp
// ✅ Specific exception types with context
catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
{
    throw new R2Exception($"Bucket '{bucketName}' does not exist.", ex);
}

// ✅ Use nameof for parameter names
throw new ArgumentException("Invalid bucket name.", nameof(bucketName));
```

#### Async Patterns
```csharp
// ✅ Async method naming and signatures
public async Task<R2ListObjectsResponse> ListObjectsAsync(
    R2ListObjectsRequest request,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Lists objects in the specified R2 bucket.
/// </summary>
/// <param name="request">The list objects request.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A task representing the list objects operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
/// <exception cref="R2Exception">Thrown when the operation fails.</exception>
/// <example>
/// <code>
/// var request = new R2ListObjectsRequest { BucketName = "my-bucket" };
/// var response = await client.ListObjectsAsync(request);
/// </code>
/// </example>
public async Task<R2ListObjectsResponse> ListObjectsAsync(
    R2ListObjectsRequest request,
    CancellationToken cancellationToken = default)
```

### Constants and Configuration

```csharp
// ✅ Use ALL_CAPS for constants
public const int DEFAULT_MAX_KEYS = 1000;
public const string DEFAULT_CONTENT_TYPE = "application/octet-stream";

// ✅ Use readonly for configuration values
private static readonly TimeSpan DEFAULT_EXPIRATION = TimeSpan.FromHours(1);
```

## Testing Guidelines

### Unit Tests

- Use xUnit as the testing framework
- Use FluentAssertions for assertions
- Use Moq for mocking dependencies
- Follow the Arrange-Act-Assert pattern

```csharp
[Fact]
public async Task ListObjectsAsync_WithValidRequest_ShouldReturnMappedObjects()
{
    // Arrange
    var request = new R2ListObjectsRequest { BucketName = "test-bucket" };
    var expectedResponse = new ListObjectsV2Response { /* ... */ };
    
    _mockS3Client.Setup(x => x.ListObjectsV2Async(
        It.IsAny<ListObjectsV2Request>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResponse);

    // Act
    var result = await _objectsClient.ListObjectsAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.BucketName.Should().Be("test-bucket");
}
```

### Test Organization

- Group tests by functionality
- Use descriptive test method names that indicate the scenario and expected outcome
- Test both success and failure scenarios
- Include edge cases and boundary conditions

### Sample Projects

When adding new features, update or create corresponding sample projects that demonstrate the functionality.

## Submitting Changes

### Workflow

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the coding standards

3. **Add tests** for new functionality

4. **Update documentation** if needed

5. **Run tests locally**:
   ```bash
   dotnet test
   ```

6. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: add support for custom metadata in objects"
   ```

7. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

8. **Create a Pull Request** on GitHub

### Commit Message Format

We use conventional commits format:

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(objects): add support for server-side encryption
fix(buckets): handle bucket already exists error correctly
docs(readme): update installation instructions
test(multipart): add tests for part validation
```

### Pull Request Guidelines

- **Title**: Use a clear, descriptive title
- **Description**: Explain what changes you made and why
- **Breaking Changes**: Clearly mark any breaking changes
- **Tests**: Ensure all tests pass
- **Documentation**: Update relevant documentation

#### Pull Request Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Manual testing completed
- [ ] Sample projects updated (if applicable)

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added to hard-to-understand areas
- [ ] Documentation updated
- [ ] No new warnings introduced
```

## Documentation

### Code Documentation

- Add XML documentation for all public APIs
- Include examples in documentation where helpful
- Document any complex algorithms or business logic

### User Documentation

- Update relevant files in the `docs/` directory
- Update sample projects when adding new features
- Keep the README.md up to date

### API Changes

For any API changes:
1. Update the corresponding documentation in `docs/`
2. Update relevant sample projects
3. Consider backward compatibility
4. Update CHANGELOG.md

## Release Process

### Versioning

We follow Semantic Versioning (SemVer):
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Changelog

Maintain CHANGELOG.md with:
- All notable changes
- Breaking changes clearly marked
- Migration guides for breaking changes

## Questions and Support

- **Issues**: Use GitHub Issues for bug reports and feature requests
- **Discussions**: Use GitHub Discussions for questions and general discussion
- **Email**: For security-related issues, contact [maintainer-email]

## Recognition

Contributors will be recognized in:
- CHANGELOG.md for significant contributions
- README.md contributors section
- GitHub contributors page

Thank you for contributing to Ebee.Cloudflare.R2!