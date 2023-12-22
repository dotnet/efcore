// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Net;
using Azure.Core;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosSingletonOptions : ICosmosSingletonOptions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AccountEndpoint { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AccountKey { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TokenCredential? TokenCredential { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? ConnectionString { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Region { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<string>? PreferredRegions { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? LimitToEndpoint { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConnectionMode? ConnectionMode { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IWebProxy? WebProxy { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? RequestTimeout { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? OpenTcpConnectionTimeout { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? IdleTcpConnectionTimeout { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GatewayModeMaxConnectionLimit { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? MaxTcpConnectionsPerEndpoint { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? MaxRequestsPerTcpConnection { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? EnableContentResponseOnWrite { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<HttpClient>? HttpClientFactory { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Initialize(IDbContextOptions options)
    {
        var cosmosOptions = options.FindExtension<CosmosOptionsExtension>();
        if (cosmosOptions != null)
        {
            AccountEndpoint = cosmosOptions.AccountEndpoint;
            AccountKey = cosmosOptions.AccountKey;
            TokenCredential = cosmosOptions.TokenCredential;
            ConnectionString = cosmosOptions.ConnectionString;
            Region = cosmosOptions.Region;
            PreferredRegions = cosmosOptions.PreferredRegions;
            LimitToEndpoint = cosmosOptions.LimitToEndpoint;
            ConnectionMode = cosmosOptions.ConnectionMode;
            WebProxy = cosmosOptions.WebProxy;
            RequestTimeout = cosmosOptions.RequestTimeout;
            OpenTcpConnectionTimeout = cosmosOptions.OpenTcpConnectionTimeout;
            IdleTcpConnectionTimeout = cosmosOptions.IdleTcpConnectionTimeout;
            GatewayModeMaxConnectionLimit = cosmosOptions.GatewayModeMaxConnectionLimit;
            MaxTcpConnectionsPerEndpoint = cosmosOptions.MaxTcpConnectionsPerEndpoint;
            MaxRequestsPerTcpConnection = cosmosOptions.MaxRequestsPerTcpConnection;
            HttpClientFactory = cosmosOptions.HttpClientFactory;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var cosmosOptions = options.FindExtension<CosmosOptionsExtension>();

        if (cosmosOptions != null
            && (AccountEndpoint != cosmosOptions.AccountEndpoint
                || AccountKey != cosmosOptions.AccountKey
                || TokenCredential != cosmosOptions.TokenCredential
                || ConnectionString != cosmosOptions.ConnectionString
                || Region != cosmosOptions.Region
                || !StructuralComparisons.StructuralEqualityComparer.Equals(PreferredRegions, cosmosOptions.PreferredRegions)
                || LimitToEndpoint != cosmosOptions.LimitToEndpoint
                || ConnectionMode != cosmosOptions.ConnectionMode
                || WebProxy != cosmosOptions.WebProxy
                || RequestTimeout != cosmosOptions.RequestTimeout
                || OpenTcpConnectionTimeout != cosmosOptions.OpenTcpConnectionTimeout
                || IdleTcpConnectionTimeout != cosmosOptions.IdleTcpConnectionTimeout
                || GatewayModeMaxConnectionLimit != cosmosOptions.GatewayModeMaxConnectionLimit
                || MaxTcpConnectionsPerEndpoint != cosmosOptions.MaxTcpConnectionsPerEndpoint
                || MaxRequestsPerTcpConnection != cosmosOptions.MaxRequestsPerTcpConnection
                || HttpClientFactory != cosmosOptions.HttpClientFactory
            ))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(CosmosDbContextOptionsExtensions.UseCosmos),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}
