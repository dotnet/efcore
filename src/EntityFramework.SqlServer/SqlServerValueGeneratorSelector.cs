// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly SimpleValueGeneratorFactory<TemporaryValueGenerator> _tempFactory;
        private readonly SqlServerSequenceValueGeneratorFactory _sequenceFactory;
        private readonly SimpleValueGeneratorFactory<SequentialGuidValueGenerator> _sequentialGuidFactory;

        public SqlServerValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryValueGenerator> tempFactory,
            [NotNull] SqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] SimpleValueGeneratorFactory<SequentialGuidValueGenerator> sequentialGuidFactory)
            : base(guidFactory)
        {
            Check.NotNull(sequenceFactory, "sequenceFactory");
            Check.NotNull(sequentialGuidFactory, "sequentialGuidFactory");

            _tempFactory = tempFactory;
            _sequenceFactory = sequenceFactory;
            _sequentialGuidFactory = sequentialGuidFactory;
        }

        public override IValueGeneratorFactory Select(IProperty property)
        {
            Check.NotNull(property, "property");

            if (property.ValueGeneration == ValueGeneration.OnAdd)
            {
                var strategy = property.SqlServer().ValueGenerationStrategy
                               ?? property.EntityType.Model.SqlServer().ValueGenerationStrategy;

                if (strategy == SqlServerValueGenerationStrategy.Sequence)
                {
                    return _sequenceFactory;
                }

                if (strategy == SqlServerValueGenerationStrategy.Identity)
                {
                    return _tempFactory;
                }

                if (property.PropertyType.IsInteger()
                    && property.PropertyType != typeof(byte))
                {
                    return _tempFactory;
                }

                if (property.PropertyType == typeof(Guid))
                {
                    return _sequentialGuidFactory;
                }
            }

            return base.Select(property);
        }
    }
}
