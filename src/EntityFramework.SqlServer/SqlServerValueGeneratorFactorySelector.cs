// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerValueGeneratorFactorySelector : ValueGeneratorFactorySelector
    {
        private readonly SqlServerSequenceValueGeneratorFactory _sequenceFactory;
        private readonly ValueGeneratorFactory<SequentialGuidValueGenerator> _sequentialGuidFactory;

        public SqlServerValueGeneratorFactorySelector(
            [NotNull] ValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] ValueGeneratorFactory<TemporaryIntegerValueGenerator> integerFactory,
            [NotNull] ValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] ValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory,
            [NotNull] SqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] ValueGeneratorFactory<SequentialGuidValueGenerator> sequentialGuidFactory)
            : base(guidFactory, integerFactory, stringFactory, binaryFactory)
        {
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(sequentialGuidFactory, nameof(sequentialGuidFactory));

            _sequenceFactory = sequenceFactory;
            _sequentialGuidFactory = sequentialGuidFactory;
        }

        public override ValueGeneratorFactory Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var strategy = property.SqlServer().ValueGenerationStrategy
                           ?? property.EntityType.Model.SqlServer().ValueGenerationStrategy;

            if (property.PropertyType.IsInteger()
                && strategy == SqlServerValueGenerationStrategy.Sequence)
            {
                return _sequenceFactory;
            }

            if (property.PropertyType == typeof(Guid))
            {
                return _sequentialGuidFactory;
            }

            return base.Select(property);
        }
    }
}
