// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryIntegerValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (property.PropertyType.UnwrapNullableType() == typeof(long))
            {
                return new InMemoryIntegerValueGenerator<long>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(int))
            {
                return new InMemoryIntegerValueGenerator<int>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(short))
            {
                return new InMemoryIntegerValueGenerator<short>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(byte))
            {
                return new InMemoryIntegerValueGenerator<byte>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(ulong))
            {
                return new InMemoryIntegerValueGenerator<ulong>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(uint))
            {
                return new InMemoryIntegerValueGenerator<uint>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(ushort))
            {
                return new InMemoryIntegerValueGenerator<ushort>();
            }

            if (property.PropertyType.UnwrapNullableType() == typeof(sbyte))
            {
                return new InMemoryIntegerValueGenerator<sbyte>();
            }

            throw new ArgumentException(Internal.Strings.InvalidValueGeneratorFactoryProperty(
                nameof(InMemoryIntegerValueGeneratorFactory), property.Name, property.EntityType.SimpleName));
        }

        public override int GetPoolSize(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return 1;
        }

        public override string GetCacheKey(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.EntityType.Name + "." + property.Name;
        }
    }
}
