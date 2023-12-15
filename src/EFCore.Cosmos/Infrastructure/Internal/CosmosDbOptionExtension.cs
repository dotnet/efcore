// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Net;
using System.Text;
using Azure.Core;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosOptionsExtension : IDbContextOptionsExtension
{
    private string? _accountEndpoint;
    private string? _accountKey;
    private TokenCredential? _tokenCredential;
    private string? _connectionString;
    private string? _databaseName;
    private string? _region;
    private IReadOnlyList<string>? _preferredRegions;
    private ConnectionMode? _connectionMode;
    private bool? _limitToEndpoint;
    private Func<ExecutionStrategyDependencies, IExecutionStrategy>? _executionStrategyFactory;
    private IWebProxy? _webProxy;
    private TimeSpan? _requestTimeout;
    private TimeSpan? _openTcpConnectionTimeout;
    private TimeSpan? _idleTcpConnectionTimeout;
    private int? _gatewayModeMaxConnectionLimit;
    private int? _maxTcpConnectionsPerEndpoint;
    private int? _maxRequestsPerTcpConnection;
    private bool? _enableContentResponseOnWrite;
    private DbContextOptionsExtensionInfo? _info;
    private Func<HttpClient>? _httpClientFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosOptionsExtension()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CosmosOptionsExtension(CosmosOptionsExtension copyFrom)
    {
        _accountEndpoint = copyFrom._accountEndpoint;
        _accountKey = copyFrom._accountKey;
        _tokenCredential = copyFrom._tokenCredential;
        _databaseName = copyFrom._databaseName;
        _connectionString = copyFrom._connectionString;
        _region = copyFrom._region;
        _preferredRegions = copyFrom._preferredRegions;
        _connectionMode = copyFrom._connectionMode;
        _limitToEndpoint = copyFrom._limitToEndpoint;
        _executionStrategyFactory = copyFrom._executionStrategyFactory;
        _webProxy = copyFrom._webProxy;
        _requestTimeout = copyFrom._requestTimeout;
        _openTcpConnectionTimeout = copyFrom._openTcpConnectionTimeout;
        _idleTcpConnectionTimeout = copyFrom._idleTcpConnectionTimeout;
        _gatewayModeMaxConnectionLimit = copyFrom._gatewayModeMaxConnectionLimit;
        _maxTcpConnectionsPerEndpoint = copyFrom._maxTcpConnectionsPerEndpoint;
        _maxRequestsPerTcpConnection = copyFrom._maxRequestsPerTcpConnection;
        _httpClientFactory = copyFrom._httpClientFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AccountEndpoint
        => _accountEndpoint;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithAccountEndpoint(string? accountEndpoint)
    {
        var clone = Clone();

        clone._accountEndpoint = accountEndpoint;
        if (accountEndpoint is not null)
        {
            clone._connectionString = null;
        }

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AccountKey
        => _accountKey;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithAccountKey(string? accountKey)
    {
        var clone = Clone();

        clone._accountKey = accountKey;
        if (accountKey is not null)
        {
            clone._connectionString = null;
            clone._tokenCredential = null;
        }

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TokenCredential? TokenCredential
        => _tokenCredential;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithTokenCredential(TokenCredential? tokenCredential)
    {
        var clone = Clone();

        clone._tokenCredential = tokenCredential;
        if (tokenCredential is not null)
        {
            clone._connectionString = null;
            clone._accountKey = null;
        }

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? ConnectionString
        => _connectionString;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithConnectionString(string? connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;
        if (connectionString is not null)
        {
            clone._accountEndpoint = null;
            clone._accountKey = null;
            clone._tokenCredential = null;
        }

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string DatabaseName
        => _databaseName!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithDatabaseName(string database)
    {
        var clone = Clone();

        clone._databaseName = database;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Region
        => _region;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithRegion(string? region)
    {
        var clone = Clone();

        clone._region = region;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<string>? PreferredRegions
        => _preferredRegions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithPreferredRegions(IReadOnlyList<string>? regions)
    {
        var clone = Clone();

        clone._preferredRegions = regions;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? LimitToEndpoint
        => _limitToEndpoint;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithLimitToEndpoint(bool enable)
    {
        var clone = Clone();

        clone._limitToEndpoint = enable;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConnectionMode? ConnectionMode
        => _connectionMode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithConnectionMode(ConnectionMode connectionMode)
    {
        if (!Enum.IsDefined(typeof(ConnectionMode), connectionMode))
        {
            throw new ArgumentOutOfRangeException(nameof(connectionMode));
        }

        var clone = Clone();

        clone._connectionMode = connectionMode;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IWebProxy? WebProxy
        => _webProxy;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithWebProxy(IWebProxy? proxy)
    {
        var clone = Clone();

        clone._webProxy = proxy;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? RequestTimeout
        => _requestTimeout;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithRequestTimeout(TimeSpan? timeout)
    {
        var clone = Clone();

        clone._requestTimeout = timeout;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? OpenTcpConnectionTimeout
        => _openTcpConnectionTimeout;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithOpenTcpConnectionTimeout(TimeSpan? timeout)
    {
        var clone = Clone();

        clone._openTcpConnectionTimeout = timeout;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? IdleTcpConnectionTimeout
        => _idleTcpConnectionTimeout;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithIdleTcpConnectionTimeout(TimeSpan? timeout)
    {
        var clone = Clone();

        clone._idleTcpConnectionTimeout = timeout;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GatewayModeMaxConnectionLimit
        => _gatewayModeMaxConnectionLimit;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithGatewayModeMaxConnectionLimit(int? connectionLimit)
    {
        var clone = Clone();

        clone._gatewayModeMaxConnectionLimit = connectionLimit;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? MaxTcpConnectionsPerEndpoint
        => _maxTcpConnectionsPerEndpoint;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithMaxTcpConnectionsPerEndpoint(int? connectionLimit)
    {
        var clone = Clone();

        clone._maxTcpConnectionsPerEndpoint = connectionLimit;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? MaxRequestsPerTcpConnection
        => _maxRequestsPerTcpConnection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithMaxRequestsPerTcpConnection(int? requestLimit)
    {
        var clone = Clone();

        clone._maxRequestsPerTcpConnection = requestLimit;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? EnableContentResponseOnWrite
        => _enableContentResponseOnWrite;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension ContentResponseOnWriteEnabled(bool enabled)
    {
        var clone = Clone();

        clone._enableContentResponseOnWrite = enabled;

        return clone;
    }

    /// <summary>
    ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <see langword="null" /> if none has been
    ///     configured.
    /// </summary>
    public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy>? ExecutionStrategyFactory
        => _executionStrategyFactory;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="executionStrategyFactory">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CosmosOptionsExtension WithExecutionStrategyFactory(
        Func<ExecutionStrategyDependencies, IExecutionStrategy>? executionStrategyFactory)
    {
        var clone = Clone();

        clone._executionStrategyFactory = executionStrategyFactory;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<HttpClient>? HttpClientFactory
        => _httpClientFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosOptionsExtension WithHttpClientFactory(Func<HttpClient>? httpClientFactory)
    {
        var clone = Clone();

        clone._httpClientFactory = httpClientFactory;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual CosmosOptionsExtension Clone()
        => new(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkCosmos();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private string? _logFragment;
        private int? _serviceProviderHash;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        private new CosmosOptionsExtension Extension
            => (CosmosOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => true;

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = new HashCode();

                if (!string.IsNullOrEmpty(Extension._connectionString))
                {
                    hashCode.Add(Extension._connectionString);
                }
                else
                {
                    hashCode.Add(Extension._accountEndpoint);
                    hashCode.Add(Extension._accountKey);
                    hashCode.Add(Extension._tokenCredential);
                }

                hashCode.Add(Extension._region);
                hashCode.Add(Extension._connectionMode);
                hashCode.Add(Extension._limitToEndpoint);
                hashCode.Add(Extension._enableContentResponseOnWrite);
                hashCode.Add(Extension._webProxy);
                hashCode.Add(Extension._requestTimeout);
                hashCode.Add(Extension._openTcpConnectionTimeout);
                hashCode.Add(Extension._idleTcpConnectionTimeout);
                hashCode.Add(Extension._gatewayModeMaxConnectionLimit);
                hashCode.Add(Extension._maxTcpConnectionsPerEndpoint);
                hashCode.Add(Extension._maxRequestsPerTcpConnection);
                hashCode.Add(Extension._httpClientFactory);

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
                && Extension._connectionString == otherInfo.Extension._connectionString
                && Extension._accountEndpoint == otherInfo.Extension._accountEndpoint
                && Extension._accountKey == otherInfo.Extension._accountKey
                && Extension._tokenCredential == otherInfo.Extension._tokenCredential
                && Extension._region == otherInfo.Extension._region
                && Extension._connectionMode == otherInfo.Extension._connectionMode
                && Extension._limitToEndpoint == otherInfo.Extension._limitToEndpoint
                && Extension._enableContentResponseOnWrite == otherInfo.Extension._enableContentResponseOnWrite
                && Extension._webProxy == otherInfo.Extension._webProxy
                && Extension._requestTimeout == otherInfo.Extension._requestTimeout
                && Extension._openTcpConnectionTimeout == otherInfo.Extension._openTcpConnectionTimeout
                && Extension._idleTcpConnectionTimeout == otherInfo.Extension._idleTcpConnectionTimeout
                && Extension._gatewayModeMaxConnectionLimit == otherInfo.Extension._gatewayModeMaxConnectionLimit
                && Extension._maxTcpConnectionsPerEndpoint == otherInfo.Extension._maxTcpConnectionsPerEndpoint
                && Extension._maxRequestsPerTcpConnection == otherInfo.Extension._maxRequestsPerTcpConnection
                && Extension._httpClientFactory == otherInfo.Extension._httpClientFactory;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            if (!string.IsNullOrEmpty(Extension._connectionString))
            {
                debugInfo["Cosmos:" + nameof(ConnectionString)] =
                    Extension._connectionString.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                debugInfo["Cosmos:" + nameof(AccountEndpoint)] =
                    (Extension._accountEndpoint?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);

                if (Extension._accountKey == null)
                {
                    debugInfo["Cosmos:" + nameof(TokenCredential)] =
                        (Extension._tokenCredential?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    debugInfo["Cosmos:" + nameof(AccountKey)] =
                        (Extension._accountKey?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                }
            }

            debugInfo["Cosmos:" + nameof(CosmosDbContextOptionsBuilder.Region)] =
                (Extension._region?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
        }

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append("ServiceEndPoint=").Append(Extension._accountEndpoint).Append(' ');

                    builder.Append("Database=").Append(Extension._databaseName).Append(' ');

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
