// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
        private readonly SqlServerValueGeneratorCache _cache;
        private readonly SqlServerSequenceValueGeneratorFactory _sequenceFactory;
        private readonly ValueGeneratorFactory<SequentialGuidValueGenerator> _sequentialGuidFactory;
        private readonly SqlServerConnection _connection;

        public SqlServerValueGeneratorSelector(
            [NotNull] SqlServerValueGeneratorCache cache,
            [NotNull] ValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] TemporaryIntegerValueGeneratorFactory integerFactory,
            [NotNull] ValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] ValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory,
            [NotNull] SqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] ValueGeneratorFactory<SequentialGuidValueGenerator> sequentialGuidFactory,
            [NotNull] SqlServerConnection connection)
            : base(guidFactory, integerFactory, stringFactory, binaryFactory)
        {
            Check.NotNull(cache, nameof(cache));
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(sequentialGuidFactory, nameof(sequentialGuidFactory));
            Check.NotNull(connection, nameof(connection));

            _cache = cache;
            _sequenceFactory = sequenceFactory;
            _sequentialGuidFactory = sequentialGuidFactory;
            _connection = connection;
        }

        public override ValueGenerator Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var strategy = property.SqlServer().ValueGenerationStrategy
                           ?? property.EntityType.Model.SqlServer().ValueGenerationStrategy;

            if (property.PropertyType.IsInteger()
                && strategy == SqlServerValueGenerationStrategy.Sequence)
            {
                return _sequenceFactory.Create(property, _cache.GetOrAddSequenceState(property), _connection);
            }

            return _cache.GetOrAdd(property, Create);
        }

        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.PropertyType == typeof(Guid) 
                ? _sequentialGuidFactory.Create(property) 
                : base.Create(property);
        }
    }
}
