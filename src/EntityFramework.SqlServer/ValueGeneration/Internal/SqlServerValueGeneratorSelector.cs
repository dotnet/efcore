// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class SqlServerValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly ISqlServerSequenceValueGeneratorFactory _sequenceFactory;

        private readonly ValueGeneratorFactory<TemporaryGuidValueGenerator> _tempraryGuidFactory
            = new ValueGeneratorFactory<TemporaryGuidValueGenerator>();

        private readonly ValueGeneratorFactory<SequentialGuidValueGenerator> _sequentialGuidFactory
            = new ValueGeneratorFactory<SequentialGuidValueGenerator>();

        private readonly ISqlServerConnection _connection;

        public SqlServerValueGeneratorSelector(
            [NotNull] ISqlServerValueGeneratorCache cache,
            [NotNull] ISqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] ISqlServerConnection connection,
            [NotNull] IRelationalAnnotationProvider relationalExtensions)
            : base(cache, relationalExtensions)
        {
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(connection, nameof(connection));

            _sequenceFactory = sequenceFactory;
            _connection = connection;
        }

        public new virtual ISqlServerValueGeneratorCache Cache => (ISqlServerValueGeneratorCache)base.Cache;

        public override ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(property, Cache.GetOrAddSequenceState(property), _connection)
                : Cache.GetOrAdd(property, entityType, Create);
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType() == typeof(Guid)
                ? (property.ValueGenerated == ValueGenerated.Never
                   || property.SqlServer().GeneratedValueSql != null)
                    ? _tempraryGuidFactory.Create(property)
                    : _sequentialGuidFactory.Create(property)
                : base.Create(property, entityType);
        }
    }
}
