// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Core;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SingletonCosmosClientWrapper : ISingletonCosmosClientWrapper
{
    private static readonly string UserAgent = " Microsoft.EntityFrameworkCore.Cosmos/" + ProductInfo.GetVersion();
    private readonly CosmosClientOptions _options;
    private readonly string? _endpoint;
    private readonly string? _key;
    private readonly string? _connectionString;
    private readonly TokenCredential? _tokenCredential;
    private CosmosClient? _client;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SingletonCosmosClientWrapper(ICosmosSingletonOptions options)
    {
        _endpoint = options.AccountEndpoint;
        _key = options.AccountKey;
        _connectionString = options.ConnectionString;
        _tokenCredential = options.TokenCredential;
        var configuration = new CosmosClientOptions { ApplicationName = UserAgent, Serializer = new JsonCosmosSerializer() };

        if (options.Region != null)
        {
            configuration.ApplicationRegion = options.Region;
        }

        if (options.PreferredRegions != null)
        {
            configuration.ApplicationPreferredRegions = options.PreferredRegions;
        }

        if (options.LimitToEndpoint != null)
        {
            configuration.LimitToEndpoint = options.LimitToEndpoint.Value;
        }

        if (options.ConnectionMode != null)
        {
            configuration.ConnectionMode = options.ConnectionMode.Value;
        }

        if (options.WebProxy != null)
        {
            configuration.WebProxy = options.WebProxy;
        }

        if (options.RequestTimeout != null)
        {
            configuration.RequestTimeout = options.RequestTimeout.Value;
        }

        if (options.OpenTcpConnectionTimeout != null)
        {
            configuration.OpenTcpConnectionTimeout = options.OpenTcpConnectionTimeout.Value;
        }

        if (options.IdleTcpConnectionTimeout != null)
        {
            configuration.IdleTcpConnectionTimeout = options.IdleTcpConnectionTimeout.Value;
        }

        if (options.GatewayModeMaxConnectionLimit != null)
        {
            configuration.GatewayModeMaxConnectionLimit = options.GatewayModeMaxConnectionLimit.Value;
        }

        if (options.MaxTcpConnectionsPerEndpoint != null)
        {
            configuration.MaxTcpConnectionsPerEndpoint = options.MaxTcpConnectionsPerEndpoint.Value;
        }

        if (options.MaxRequestsPerTcpConnection != null)
        {
            configuration.MaxRequestsPerTcpConnection = options.MaxRequestsPerTcpConnection.Value;
        }

        if (options.HttpClientFactory != null)
        {
            configuration.HttpClientFactory = options.HttpClientFactory;
        }

        _options = configuration;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosClient Client
        => _client ??= string.IsNullOrEmpty(_connectionString)
            ? _tokenCredential == null
                ? _endpoint == null
                    ? throw new InvalidOperationException(CosmosStrings.ConnectionInfoMissing)
                    : new CosmosClient(_endpoint, _key, _options)
                : new CosmosClient(_endpoint, _tokenCredential, _options)
            : new CosmosClient(_connectionString, _options);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Dispose()
    {
        _client?.Dispose();
        _client = null;
    }
}
