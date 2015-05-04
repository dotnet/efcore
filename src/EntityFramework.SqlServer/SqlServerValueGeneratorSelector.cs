// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly ISqlServerSequenceValueGeneratorFactory _sequenceFactory;

        private readonly ValueGeneratorFactory<SequentialGuidValueGenerator> _sequentialGuidFactory
            = new ValueGeneratorFactory<SequentialGuidValueGenerator>();

        private readonly ISqlServerConnection _connection;

        public SqlServerValueGeneratorSelector(
            [NotNull] ISqlServerValueGeneratorCache cache,
            [NotNull] ISqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] ISqlServerConnection connection)
            : base(cache)
        {
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(connection, nameof(connection));

            _sequenceFactory = sequenceFactory;
            _connection = connection;
        }

        public virtual new ISqlServerValueGeneratorCache Cache => (ISqlServerValueGeneratorCache)base.Cache;

        public override ValueGenerator Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var strategy = property.SqlServer().ValueGenerationStrategy;

            return property.ClrType.IsInteger()
                   && strategy == SqlServerValueGenerationStrategy.Sequence
                ? _sequenceFactory.Create(property, Cache.GetOrAddSequenceState(property), _connection)
                : Cache.GetOrAdd(property, Create);
        }

        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.ClrType == typeof(Guid)
                ? _sequentialGuidFactory.Create(property)
                : base.Create(property);
        }
    }
}
