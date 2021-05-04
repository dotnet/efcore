// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     <para>
    ///         Factory for creation of temporary integer value generators appropriate
    ///         for the numeric type of the property.
    ///     </para>
    ///     <para>
    ///         Types supported are: <see cref="int" />, <see cref="long" />, <see cref="short" />, <see cref="byte" />,
    ///         <see cref="char" />, <see cref="ulong" />, <see cref="uint" />, <see cref="ushort" />, <see cref="sbyte" />,
    ///         <see cref="decimal" />, <see cref="float" />, <see cref="double" />
    ///     </para>
    /// </summary>
    public class TemporaryNumberValueGeneratorFactory : ValueGeneratorFactory
    {
        /// <summary>
        ///     Creates a new value generator.
        /// </summary>
        /// <param name="property"> The property to create the value generator for. </param>
        /// <param name="entityType"> The entity type for which the value generator will be used. </param>
        /// <returns> The newly created value generator. </returns>
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
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
