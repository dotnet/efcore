// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal
{
    public class CosmosOptionsExtension : IDbContextOptionsExtension
    {
        private string _serviceEndPoint;
        private string _authKeyOrResourceToken;
        private string _region;
        private string _databaseName;
        private Func<ExecutionStrategyDependencies, IExecutionStrategy> _executionStrategyFactory;
        private DbContextOptionsExtensionInfo _info;

        public CosmosOptionsExtension()
        {
        }

        protected CosmosOptionsExtension(CosmosOptionsExtension copyFrom)
        {
            _serviceEndPoint = copyFrom._serviceEndPoint;
            _authKeyOrResourceToken = copyFrom._authKeyOrResourceToken;
            _databaseName = copyFrom._databaseName;
            _executionStrategyFactory = copyFrom._executionStrategyFactory;
            _region = copyFrom._region;
        }

        public virtual DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        public virtual string ServiceEndPoint => _serviceEndPoint;

        public virtual CosmosOptionsExtension WithServiceEndPoint(string serviceEndPoint)
        {
            var clone = Clone();

            clone._serviceEndPoint = serviceEndPoint;

            return clone;
        }

        public virtual string AuthKeyOrResourceToken => _authKeyOrResourceToken;

        public virtual CosmosOptionsExtension WithAuthKeyOrResourceToken(string authKeyOrResourceToken)
        {
            var clone = Clone();

            clone._authKeyOrResourceToken = authKeyOrResourceToken;

            return clone;
        }

        public virtual string DatabaseName => _databaseName;

        public virtual CosmosOptionsExtension WithDatabaseName(string database)
        {
            var clone = Clone();

            clone._databaseName = database;

            return clone;
        }

        public virtual string Region => _region;

        public virtual CosmosOptionsExtension WithRegion(string region)
        {
            var clone = Clone();

            clone._region = region;

            return clone;
        }

        /// <summary>
        ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <c>null</c> if none has been
        ///     configured.
        /// </summary>
        public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy> ExecutionStrategyFactory => _executionStrategyFactory;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="executionStrategyFactory"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CosmosOptionsExtension WithExecutionStrategyFactory(
            [CanBeNull] Func<ExecutionStrategyDependencies, IExecutionStrategy> executionStrategyFactory)
        {
            var clone = Clone();

            clone._executionStrategyFactory = executionStrategyFactory;

            return clone;
        }

        protected virtual CosmosOptionsExtension Clone() => new CosmosOptionsExtension(this);

        public void ApplyServices(IServiceCollection services)
            => services.AddEntityFrameworkCosmos();

        public void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string _logFragment;
            private long? _serviceProviderHash;

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new CosmosOptionsExtension Extension
                => (CosmosOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => true;

            public override long GetServiceProviderHashCode()
            {
                if (_serviceProviderHash == null)
                {
                    var hashCode = Extension._serviceEndPoint.GetHashCode();
                    hashCode = (hashCode * 397) ^ Extension._authKeyOrResourceToken.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Extension._region?.GetHashCode() ?? 0);

                    _serviceProviderHash = hashCode;
                }

                return _serviceProviderHash.Value;
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                Check.NotNull(debugInfo, nameof(debugInfo));

                debugInfo["Cosmos:" + nameof(ServiceEndPoint)] = Extension._serviceEndPoint.GetHashCode().ToString(CultureInfo.InvariantCulture);
                debugInfo["Cosmos:" + nameof(AuthKeyOrResourceToken)] = Extension._authKeyOrResourceToken.GetHashCode().ToString(CultureInfo.InvariantCulture);
                debugInfo["Cosmos:" + nameof(CosmosDbContextOptionsBuilder.Region)] = (Extension._region?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
            }

            public override string LogFragment
            {
                get
                {
                    if (_logFragment == null)
                    {
                        var builder = new StringBuilder();

                        builder.Append("ServiceEndPoint=").Append(Extension._serviceEndPoint).Append(' ');

                        builder.Append("Database=").Append(Extension._databaseName).Append(' ');

                        _logFragment = builder.ToString();
                    }

                    return _logFragment;
                }
            }
        }
    }
}
