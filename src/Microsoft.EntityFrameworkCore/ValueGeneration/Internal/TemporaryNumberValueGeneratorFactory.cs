// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TemporaryNumberValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property)
        {
            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(int))
            {
                return new TemporaryIntValueGenerator();
            }

            if (type == typeof(long))
            {
                return new TemporaryLongValueGenerator();
            }

            if (type == typeof(short))
            {
                return new TemporaryShortValueGenerator();
            }

            if (type == typeof(byte))
            {
                return new TemporaryByteValueGenerator();
            }

            if (type == typeof(char))
            {
                return new TemporaryCharValueGenerator();
            }

            if (type == typeof(ulong))
            {
                return new TemporaryULongValueGenerator();
            }

            if (type == typeof(uint))
            {
                return new TemporaryUIntValueGenerator();
            }

            if (type == typeof(ushort))
            {
                return new TemporaryUShortValueGenerator();
            }

            if (type == typeof(sbyte))
            {
                return new TemporarySByteValueGenerator();
            }

            if (type == typeof(decimal))
            {
                return new TemporaryDecimalValueGenerator();
            }

            if (type == typeof(float))
            {
                return new TemporaryFloatValueGenerator();
            }

            if (type == typeof(double))
            {
                return new TemporaryDoubleValueGenerator();
            }

            throw new ArgumentException(CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(TemporaryNumberValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
