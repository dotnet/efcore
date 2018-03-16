// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryIntegerValueGeneratorFactory : ValueGeneratorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

            throw new ArgumentException(
                CoreStrings.InvalidValueGeneratorFactoryProperty(
                    nameof(InMemoryIntegerValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
