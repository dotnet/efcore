// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class SingletonCosmosClientWrapper : IDisposable
    {
        private static readonly string _userAgent = " Microsoft.EntityFrameworkCore.Cosmos/" + ProductInfo.GetVersion();
        private readonly CosmosClientOptions _options;
        private readonly string _endpoint;
        private readonly string _key;
        private readonly string _connectionString;
        private CosmosClient _client;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SingletonCosmosClientWrapper([NotNull] ICosmosSingletonOptions options)
        {
            _endpoint = options.AccountEndpoint;
            _key = options.AccountKey;
            _connectionString = options.ConnectionString;
            var configuration = new CosmosClientOptions { ApplicationName = _userAgent, Serializer = new JsonCosmosSerializer() };

            if (options.Region != null)
            {
                configuration.ApplicationRegion = options.Region;
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
                ? new CosmosClient(_endpoint, _key, _options)
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
}
