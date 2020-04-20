// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SqlServerValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly ISqlServerSequenceValueGeneratorFactory _sequenceFactory;
        private readonly ISqlServerConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerValueGeneratorSelector(
            [NotNull] ValueGeneratorSelectorDependencies dependencies,
            [NotNull] ISqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] ISqlServerConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
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
        public new virtual ISqlServerValueGeneratorCache Cache => (ISqlServerValueGeneratorCache)base.Cache;

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

            // TODO Dirty hack
            var converter = property.GetValueConverter();
            if (converter != null && property is Property p)
            {
                var fieldInfo = p.GetType().GetField("<ClrType>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

                var origPropertyType = p.ClrType;
                try
                {
                    fieldInfo.SetValue(p, converter.ProviderClrType);
                    var valueGenerator = SelectInternal(p, entityType);
                    return new ConverterValueGeneratorAdapter(valueGenerator, converter);
                }
                finally
                {
                    fieldInfo.SetValue(p, origPropertyType);
                }
            }

            return SelectInternal(property, entityType);
        }

        private ValueGenerator SelectInternal(IProperty property, IEntityType entityType)
        {
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
        protected override ValueGenerator CreateFromFactory(IProperty property, IEntityType entityType)
        {
            var factory = property.GetValueGeneratorFactory();

            if (factory == null)
            {
                var mapping = property.FindTypeMapping();
                factory = mapping?.ValueGeneratorFactory;
            }

            return factory?.Invoke(property, entityType);
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
                    ? (ValueGenerator)new TemporaryGuidValueGenerator()
                    : new SequentialGuidValueGenerator()
                : base.Create(property, entityType);
        }
    }

    // TODO Move this to the appropriate place
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </summary>
    public class ConverterValueGeneratorAdapter : ValueGenerator
    {
        private readonly ValueGenerator _generator;
        private readonly ValueConverter _converter;

        public ConverterValueGeneratorAdapter(
            [NotNull] ValueGenerator generator,
            [NotNull] ValueConverter converter)
        {
            _generator = generator;
            _converter = converter;
        }

        /// <summary>
        ///     Template method to be overridden by implementations to perform value generation.
        /// </summary>
        /// <param name="entry"> The change tracking entry of the entity for which the value is being generated. </param>
        /// <returns> The generated value. </returns>
        protected override object NextValue(EntityEntry entry)
        {
            return _converter.ConvertFromProvider.Invoke(_generator.Next(entry));
        }

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether the values generated are temporary (i.e they should be replaced
        ///         by database generated values when the entity is saved) or are permanent (i.e. the generated values
        ///         should be saved to the database).
        ///     </para>
        ///     <para>
        ///         An example of temporary value generation is generating negative numbers for an integer primary key
        ///         that are then replaced by positive numbers generated by the database when the entity is saved. An
        ///         example of permanent value generation are client-generated values for a <see cref="Guid" /> primary
        ///         key which are saved to the database.
        ///     </para>
        /// </summary>
        public override bool GeneratesTemporaryValues => _generator.GeneratesTemporaryValues;
    }
}
