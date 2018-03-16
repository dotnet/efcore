// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Oracle.ValueGeneration.Internal
{
    public class OracleValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly IOracleSequenceValueGeneratorFactory _sequenceFactory;

        private readonly IOracleConnection _connection;

        public OracleValueGeneratorSelector(
            [NotNull] ValueGeneratorSelectorDependencies dependencies,
            [NotNull] IOracleSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] IOracleConnection connection)
            : base(dependencies)
        {
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(connection, nameof(connection));

            _sequenceFactory = sequenceFactory;
            _connection = connection;
        }

        public new virtual IOracleValueGeneratorCache Cache => (IOracleValueGeneratorCache)base.Cache;

        public override ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.GetValueGeneratorFactory() == null
                   && property.Oracle().ValueGenerationStrategy == OracleValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(property, Cache.GetOrAddSequenceState(property), _connection)
                : base.Select(property, entityType);
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType() == typeof(Guid)
                ? property.ValueGenerated == ValueGenerated.Never
                  || property.Oracle().DefaultValueSql != null
                    ? new TemporaryGuidValueGenerator()
                    : new GuidValueGenerator()
                : base.Create(property, entityType);
        }
    }
}
