// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly SqlServerSequenceValueGeneratorFactory _sequenceFactory;
        private readonly SimpleValueGeneratorFactory<SequentialGuidValueGenerator> _sequentialGuidFactory;

        public SqlServerValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator> integerFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory,
            [NotNull] SqlServerSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] SimpleValueGeneratorFactory<SequentialGuidValueGenerator> sequentialGuidFactory)
            : base(guidFactory, integerFactory, stringFactory, binaryFactory)
        {
            Check.NotNull(sequenceFactory, "sequenceFactory");
            Check.NotNull(sequentialGuidFactory, "sequentialGuidFactory");

            _sequenceFactory = sequenceFactory;
            _sequentialGuidFactory = sequentialGuidFactory;
        }

        public override IValueGeneratorFactory Select(IProperty property)
        {
            Check.NotNull(property, "property");

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
