# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial implementation of comprehensive Cloudflare R2 client library

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [1.0.0] - 2025-10-04

### Added
- **Core R2Client**: Main client providing unified access to all R2 operations
- **Bucket Operations**: Complete bucket management functionality
  - Create buckets with validation and error handling
  - List all buckets in account with owner information
  - Delete buckets with proper error handling for non-empty buckets
- **Object Operations**: Full object lifecycle management
  - Upload objects from byte arrays, streams, or file paths
  - Download objects with content streaming and metadata support
  - List objects with pagination, filtering, and prefix support
  - Copy objects within or between buckets with metadata handling
  - Delete objects with version support
  - Get object metadata without downloading content
  - Support for conditional operations (If-Match, If-None-Match, etc.)
  - Range downloads for partial content retrieval
- **Signed URLs**: Pre-signed URL generation for secure access
  - Generate GET URLs for downloading objects
  - Generate PUT URLs for uploading objects
  - Generate DELETE URLs for removing objects
  - Configurable expiration times with validation
  - Response header overrides for GET URLs
  - Metadata and encryption support for PUT URLs
- **Multipart Uploads**: Efficient large file upload handling
  - Initiate multipart uploads with metadata and encryption
  - Upload parts with multiple content sources (stream, bytes, file path)
  - Complete multipart uploads with part validation
  - Abort multipart uploads for cleanup
  - List parts of ongoing uploads with pagination
  - List all multipart uploads in bucket with filtering
  - Support for server-side encryption with customer keys
  - Part number validation (1-10,000 parts)
- **Server-Side Encryption**: Comprehensive encryption support
  - SSE-S3 (AES256) encryption
  - SSE-C (customer-provided keys) with MD5 validation
  - Encryption support across all operations
- **Metadata Management**: Rich metadata handling
  - Custom metadata for objects and uploads
  - Automatic metadata prefixing for S3 compatibility
  - Metadata copying and replacement during object operations
- **Error Handling**: Robust error management
  - Custom R2Exception with detailed error messages
  - Specific error handling for common scenarios (NoSuchBucket, NoSuchKey, etc.)
  - Inner exception preservation for debugging
  - Contextual error messages with operation details
- **Dependency Injection**: Built-in DI support
  - Extension methods for service registration
  - Configuration through options pattern
  - Environment variable support
  - Proper lifetime management for all services
- **Documentation**: Comprehensive documentation suite
  - Detailed API documentation with examples
  - Individual documentation files for each client
  - Working sample projects for all operations
  - Contributing guidelines and development setup
- **Testing**: Extensive test coverage
  - Unit tests for all client operations
  - Mock-based testing with FluentAssertions
  - Error scenario validation
  - Edge case handling verification

### Technical Details
- **.NET 8**: Built for .NET 8 with modern C# features
  - Primary constructors for dependency injection
  - File-scoped namespaces
  - Collection expressions
  - Required properties for request models
  - Pattern matching and switch expressions
- **AWS SDK Integration**: Built on top of AWS SDK for .NET
  - Full S3 API compatibility for R2
  - Proper request/response mapping
  - Stream handling and resource management
- **Async/Await**: Complete asynchronous API
  - All operations support cancellation tokens
  - Proper async patterns throughout
- **Resource Management**: Proper resource handling
  - IDisposable implementation for response objects
  - Stream disposal and cleanup
  - Memory-efficient operations

### Performance Features
- **Streaming Support**: Memory-efficient large file handling
- **Parallel Operations**: Support for concurrent part uploads
- **Pagination**: Efficient handling of large result sets
- **Connection Reuse**: Optimized HTTP client usage

### Security Features
- **Input Validation**: Comprehensive parameter validation
- **Safe Defaults**: Secure default configurations
- **Error Information**: Safe error messages without sensitive data exposure

## Migration Guide

### From AWS S3 SDK
If you're migrating from direct AWS S3 SDK usage:

1. Replace `AmazonS3Client` with `IR2Client`
2. Update request/response models to use R2-specific types
3. Handle R2Exception instead of AmazonS3Exception
4. Use the new dependency injection setup

### Configuration Changes
- Use `AddR2Client()` extension method for DI registration
- Set account ID in addition to access keys

## Breaking Changes

This is the initial release, so no breaking changes apply.

## Dependencies

- **AWSSDK.S3**: 4.0.7.7 - Core S3 API functionality
- **Microsoft.Extensions.DependencyInjection.Abstractions**: ~8.0.0 - DI support
- **Microsoft.Extensions.Options**: ~8.0.0 - Configuration support

## Supported Platforms

- .NET 8.0 and later
- Windows, macOS, and Linux
- Compatible with all major cloud platforms

## Known Issues

- None at this time

## Acknowledgments

- Built on the robust AWS SDK for .NET
- Inspired by Cloudflare R2's S3-compatible API
- Community feedback and contributions

---

For more details about any release, please check the [GitHub releases page](https://github.com/ebeeraheem/Ebee.Cloudflare.R2/releases).