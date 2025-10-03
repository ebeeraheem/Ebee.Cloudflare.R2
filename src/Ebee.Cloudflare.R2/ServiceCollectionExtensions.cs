using Amazon.S3;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ebee.Cloudflare.R2.Buckets;
using Ebee.Cloudflare.R2.Objects;
using Ebee.Cloudflare.R2.SignedUrls;

namespace Ebee.Cloudflare.R2;

/// <summary>
/// Extension methods for service collection to register R2 client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds R2 client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for R2 options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddR2Client(options =>
    /// {
    ///     options.AccountId = "your-account-id";
    ///     options.AccessKeyId = "your-access-key";
    ///     options.SecretAccessKey = "your-secret-key";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddR2Client(this IServiceCollection services, Action<R2Options> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new R2Options()
        {
            AccountId = string.Empty,
            AccessKeyId = string.Empty,
            SecretAccessKey = string.Empty,
        };

        configure(options);

        return services.AddR2ClientCore(options);
    }

    /// <summary>
    /// Adds R2 client services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The configuration section name. Defaults to "R2".</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddR2Client(configuration);
    /// // or with custom section name
    /// services.AddR2Client(configuration, "CloudflareR2");
    /// </code>
    /// </example>
    public static IServiceCollection AddR2Client(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "R2")
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration.GetSection(sectionName).Get<R2Options>()
            ?? throw new InvalidOperationException($"R2 configuration section '{sectionName}' not found or invalid.");

        return services.AddR2ClientCore(options);
    }

    /// <summary>
    /// Adds R2 client services to the service collection with a named client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The name of the client.</param>
    /// <param name="configure">Configuration action for R2 options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddR2Client("production", options =>
    /// {
    ///     options.AccountId = "prod-account-id";
    ///     options.AccessKeyId = "prod-access-key";
    ///     options.SecretAccessKey = "prod-secret-key";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddR2Client(
        this IServiceCollection services,
        string name,
        Action<R2Options> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new R2Options()
        {
            AccountId = string.Empty,
            AccessKeyId = string.Empty,
            SecretAccessKey = string.Empty,
        };

        configure(options);

        return services.AddR2ClientCore(options, name);
    }

    private static IServiceCollection AddR2ClientCore(
        this IServiceCollection services,
        R2Options options,
        string? name = null)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = options.GetEndpointUrl(),
            ForcePathStyle = true,
            UseHttp = false
        };

        AWSConfigsS3.UseSignatureVersion4 = true;

        if (string.IsNullOrEmpty(name))
        {
            services.AddSingleton<IAmazonS3>(_ =>
                new AmazonS3Client(options.AccessKeyId, options.SecretAccessKey, config));

            services.AddScoped<IBucketsClient, BucketsClient>();
            services.AddScoped<IObjectsClient, ObjectsClient>();
            services.AddScoped<ISignedUrlsClient, SignedUrlsClient>();
            services.AddScoped<IR2Client, R2Client>();
        }
        else
        {
            services.AddKeyedSingleton<IAmazonS3>(name, (_, _) =>
                new AmazonS3Client(options.AccessKeyId, options.SecretAccessKey, config));

            services.AddKeyedScoped<IBucketsClient, BucketsClient>(name);
            services.AddKeyedScoped<IObjectsClient, ObjectsClient>(name);
            services.AddKeyedScoped<ISignedUrlsClient, SignedUrlsClient>(name);
            services.AddKeyedScoped<IR2Client, R2Client>(name);
        }

        return services;
    }
}
