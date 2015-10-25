// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class InMemoryIntegerValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return new InMemoryIntegerValueGenerator<long>();
            }

            if (type == typeof(int))
            {
                return new InMemoryIntegerValueGenerator<int>();
            }

            if (type == typeof(short))
            {
                return new InMemoryIntegerValueGenerator<short>();
            }

            if (type == typeof(byte))
            {
                return new InMemoryIntegerValueGenerator<byte>();
            }

            if (type == typeof(ulong))
            {
                return new InMemoryIntegerValueGenerator<ulong>();
            }

            if (type == typeof(uint))
            {
                return new InMemoryIntegerValueGenerator<uint>();
            }

            if (type == typeof(ushort))
            {
                return new InMemoryIntegerValueGenerator<ushort>();
            }

            if (type == typeof(sbyte))
            {
                return new InMemoryIntegerValueGenerator<sbyte>();
            }

            throw new ArgumentException(CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(InMemoryIntegerValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
