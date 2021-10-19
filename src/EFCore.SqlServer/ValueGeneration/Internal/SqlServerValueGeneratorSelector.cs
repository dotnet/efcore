// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///     <see cref="DbContext" /> instance will use its own instance of this service.
    ///     The implementation may depend on other services registered with any lifetime.
    ///     The implementation does not need to be thread-safe.
    /// </remarks>
    public class SqlServerValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly ISqlServerSequenceValueGeneratorFactory _sequenceFactory;
        private readonly ISqlServerConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerValueGeneratorSelector(
            ValueGeneratorSelectorDependencies dependencies,
            ISqlServerSequenceValueGeneratorFactory sequenceFactory,
            ISqlServerConnection connection,
            IRawSqlCommandBuilder rawSqlCommandBuilder,
            IRelationalCommandDiagnosticsLogger commandLogger)
            : base(dependencies)
        {
            _sequenceFactory = sequenceFactory;
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _commandLogger = commandLogger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual ISqlServerValueGeneratorCache Cache
            => (ISqlServerValueGeneratorCache)base.Cache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.GetValueGeneratorFactory() == null
                && property.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.SequenceHiLo
                    ? _sequenceFactory.Create(
                        property,
                        Cache.GetOrAddSequenceState(property, _connection),
                        _connection,
                        _rawSqlCommandBuilder,
                        _commandLogger)
                    : base.Select(property, entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType() == typeof(Guid)
                ? property.ValueGenerated == ValueGenerated.Never || property.GetDefaultValueSql() != null
                    ? new TemporaryGuidValueGenerator()
                    : new SequentialGuidValueGenerator()
                : base.Create(property, entityType);
        }
    }
}
