// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class TemporaryNumberValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var type = property.ClrType.UnwrapNullableType();

            if (type == typeof(long))
            {
                return new TemporaryNumberValueGenerator<long>();
            }

            if (type == typeof(int))
            {
                return new TemporaryNumberValueGenerator<int>();
            }

            if (type == typeof(short))
            {
                return new TemporaryNumberValueGenerator<short>();
            }

            if (type == typeof(byte))
            {
                return new TemporaryNumberValueGenerator<byte>();
            }

            if (type == typeof(char))
            {
                return new TemporaryNumberValueGenerator<char>();
            }

            if (type == typeof(ulong))
            {
                return new TemporaryNumberValueGenerator<ulong>();
            }

            if (type == typeof(uint))
            {
                return new TemporaryNumberValueGenerator<uint>();
            }

            if (type == typeof(ushort))
            {
                return new TemporaryNumberValueGenerator<ushort>();
            }

            if (type == typeof(sbyte))
            {
                return new TemporaryNumberValueGenerator<sbyte>();
            }

            if (type == typeof(decimal))
            {
                return new TemporaryNumberValueGenerator<decimal>();
            }

            if (type == typeof(float))
            {
                return new TemporaryNumberValueGenerator<float>();
            }

            if (type == typeof(double))
            {
                return new TemporaryNumberValueGenerator<double>();
            }

            throw new ArgumentException(Strings.InvalidValueGeneratorFactoryProperty(
                nameof(TemporaryNumberValueGeneratorFactory), property.Name, property.EntityType.DisplayName()));
        }
    }
}
