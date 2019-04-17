// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TemporaryNumberValueGeneratorFactory : ValueGeneratorFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

            throw new ArgumentException(
                CoreStrings.InvalidValueGeneratorFactoryProperty(
                    nameof(TemporaryNumberValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
