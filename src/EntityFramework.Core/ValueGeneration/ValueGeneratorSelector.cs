// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGeneratorSelector : IValueGeneratorSelector
    {
        private readonly ValueGeneratorFactory<GuidValueGenerator> _guidFactory
            = new ValueGeneratorFactory<GuidValueGenerator>();

        private readonly TemporaryIntegerValueGeneratorFactory _integerFactory
            = new TemporaryIntegerValueGeneratorFactory();

        private readonly ValueGeneratorFactory<TemporaryStringValueGenerator> _stringFactory
            = new ValueGeneratorFactory<TemporaryStringValueGenerator>();

        private readonly ValueGeneratorFactory<TemporaryBinaryValueGenerator> _binaryFactory
            = new ValueGeneratorFactory<TemporaryBinaryValueGenerator>();

        public abstract ValueGenerator Select(IProperty property);

        public virtual ValueGenerator Create([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var propertyType = property.ClrType.UnwrapNullableType();
            if (propertyType == typeof(Guid))
            {
                return _guidFactory.Create(property);
            }

            if (propertyType.UnwrapNullableType().IsInteger())
            {
                return _integerFactory.Create(property);
            }

            if (propertyType == typeof(string))
            {
                return _stringFactory.Create(property);
            }

            if (propertyType == typeof(byte[]))
            {
                return _binaryFactory.Create(property);
            }

            throw new NotSupportedException(
                Strings.NoValueGenerator(property.Name, property.EntityType.DisplayName(), propertyType.Name));
        }
    }
}
