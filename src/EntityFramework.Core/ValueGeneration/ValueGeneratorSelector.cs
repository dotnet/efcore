// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class ValueGeneratorSelector : IValueGeneratorSelector
    {
        private readonly ValueGeneratorFactory<GuidValueGenerator> _guidFactory
            = new ValueGeneratorFactory<GuidValueGenerator>();

        private readonly TemporaryNumberValueGeneratorFactory _numberFactory
            = new TemporaryNumberValueGeneratorFactory();

        private readonly ValueGeneratorFactory<TemporaryStringValueGenerator> _stringFactory
            = new ValueGeneratorFactory<TemporaryStringValueGenerator>();

        private readonly ValueGeneratorFactory<TemporaryBinaryValueGenerator> _binaryFactory
            = new ValueGeneratorFactory<TemporaryBinaryValueGenerator>();

        private readonly ValueGeneratorFactory<TemporaryDateTimeValueGenerator> _dateTimeFactory
            = new ValueGeneratorFactory<TemporaryDateTimeValueGenerator>();

        private readonly ValueGeneratorFactory<TemporaryDateTimeOffsetValueGenerator> _dateTimeOffsetFactory
            = new ValueGeneratorFactory<TemporaryDateTimeOffsetValueGenerator>();

        public virtual IValueGeneratorCache Cache { get; }

        public ValueGeneratorSelector([NotNull] IValueGeneratorCache cache)
        {
            Check.NotNull(cache, nameof(cache));

            Cache = cache;
        }

        public virtual ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return Cache.GetOrAdd(property, entityType, Create);
        }

        public virtual ValueGenerator Create([NotNull] IProperty property, [NotNull] IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            var propertyType = property.ClrType.UnwrapNullableType();

            if (propertyType == typeof(Guid))
            {
                return _guidFactory.Create(property);
            }

            if (propertyType.IsInteger()
                || propertyType == typeof(decimal)
                || propertyType == typeof(float)
                || propertyType == typeof(double))
            {
                return _numberFactory.Create(property);
            }

            if (propertyType == typeof(string))
            {
                return _stringFactory.Create(property);
            }

            if (propertyType == typeof(byte[]))
            {
                return _binaryFactory.Create(property);
            }

            if (propertyType == typeof(DateTime))
            {
                return _dateTimeFactory.Create(property);
            }

            if (propertyType == typeof(DateTimeOffset))
            {
                return _dateTimeOffsetFactory.Create(property);
            }

            throw new NotSupportedException(
                Strings.NoValueGenerator(property.Name, property.EntityType.DisplayName(), propertyType.Name));
        }
    }
}
