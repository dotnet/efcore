// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure.Internal
{
    public class CosmosSqlDbOptionsExtension : IDbContextOptionsExtension
    {
        private Uri _serviceEndPoint;
        private string _authKeyOrResourceToken;
        private string _databaseName;
        private Func<ExecutionStrategyDependencies, IExecutionStrategy> _executionStrategyFactory;
        private string _logFragment;

        public CosmosSqlDbOptionsExtension()
        {
        }

        protected CosmosSqlDbOptionsExtension(CosmosSqlDbOptionsExtension copyFrom)
        {
            _serviceEndPoint = copyFrom._serviceEndPoint;
            _authKeyOrResourceToken = copyFrom._authKeyOrResourceToken;
            _databaseName = copyFrom._databaseName;
            _executionStrategyFactory = copyFrom._executionStrategyFactory;
        }

        public virtual Uri ServiceEndPoint => _serviceEndPoint;

        public virtual CosmosSqlDbOptionsExtension WithServiceEndPoint(Uri serviceEndPoint)
        {
            var clone = Clone();

            clone._serviceEndPoint = serviceEndPoint;

            return clone;
        }

        public virtual string AuthKeyOrResourceToken => _authKeyOrResourceToken;

        public virtual CosmosSqlDbOptionsExtension WithAuthKeyOrResourceToken(string authKeyOrResourceToken)
        {
            var clone = Clone();

            clone._authKeyOrResourceToken = authKeyOrResourceToken;

            return clone;
        }

        public virtual string DatabaseName => _databaseName;

        public virtual CosmosSqlDbOptionsExtension WithDatabaseName(string database)
        {
            var clone = Clone();

            clone._databaseName = database;

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
        public virtual CosmosSqlDbOptionsExtension WithExecutionStrategyFactory(
            [CanBeNull] Func<ExecutionStrategyDependencies, IExecutionStrategy> executionStrategyFactory)
        {
            var clone = Clone();

            clone._executionStrategyFactory = executionStrategyFactory;

            return clone;
        }

        protected virtual CosmosSqlDbOptionsExtension Clone() => new CosmosSqlDbOptionsExtension(this);

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkCosmosSql();

            return true;
        }

        public long GetServiceProviderHashCode()
        {
            return 0;
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append("ServiceEndPoint=").Append(_serviceEndPoint).Append(' ');

                    builder.Append("Database=").Append(_databaseName).Append(' ');

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
