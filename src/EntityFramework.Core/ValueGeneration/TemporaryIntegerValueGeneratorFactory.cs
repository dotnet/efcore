// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class TemporaryIntegerValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (property.ClrType.UnwrapNullableType() == typeof(long))
            {
                return new TemporaryIntegerValueGenerator<long>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(int))
            {
                return new TemporaryIntegerValueGenerator<int>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(short))
            {
                return new TemporaryIntegerValueGenerator<short>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(byte))
            {
                return new TemporaryIntegerValueGenerator<byte>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ulong))
            {
                return new TemporaryIntegerValueGenerator<ulong>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(uint))
            {
                return new TemporaryIntegerValueGenerator<uint>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(ushort))
            {
                return new TemporaryIntegerValueGenerator<ushort>();
            }

            if (property.ClrType.UnwrapNullableType() == typeof(sbyte))
            {
                return new TemporaryIntegerValueGenerator<sbyte>();
            }

            throw new ArgumentException(Strings.InvalidValueGeneratorFactoryProperty(
                nameof(TemporaryIntegerValueGeneratorFactory), property.Name, property.EntityType.DisplayName()));
        }
    }
}
