// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqlServerValueGeneratorCache : ValueGeneratorCache, ISqlServerValueGeneratorCache
    {
        private readonly ConcurrentDictionary<string, SqlServerSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ConcurrentDictionary<string, SqlServerSequenceValueGeneratorState>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public SqlServerValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual SqlServerSequenceValueGeneratorState GetOrAddSequenceState(
            IProperty property,
            IRelationalConnection connection)
        {
            var sequence = property.SqlServer().FindHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence, connection),
                sequenceName => new SqlServerSequenceValueGeneratorState(sequence));
        }

        private static string GetSequenceName(ISequence sequence, IRelationalConnection connection)
        {
            var dbConnection = connection.DbConnection;

            return dbConnection.Database.ToUpperInvariant()
                   + "::"
                   + dbConnection.DataSource?.ToUpperInvariant()
                   + "::"
                   + (sequence.Schema == null ? "" : sequence.Schema + ".") + sequence.Name;
        }
    }
}
