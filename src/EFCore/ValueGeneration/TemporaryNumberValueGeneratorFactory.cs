// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     Factory for creation of temporary integer value generators appropriate
///     for the numeric type of the property.
/// </summary>
/// <remarks>
///     Types supported are: <see cref="int" />, <see cref="long" />, <see cref="short" />, <see cref="byte" />,
///     <see cref="char" />, <see cref="ulong" />, <see cref="uint" />, <see cref="ushort" />, <see cref="sbyte" />,
///     <see cref="decimal" />, <see cref="float" />, <see cref="double" />
/// </remarks>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public class TemporaryNumberValueGeneratorFactory : ValueGeneratorFactory
{
    /// <summary>
    ///     Creates a new value generator.
    /// </summary>
    /// <param name="property">The property to create the value generator for.</param>
    /// <param name="entityType">The type for which the value generator will be used.</param>
    /// <returns>The newly created value generator.</returns>
    public override ValueGenerator Create(IProperty property, ITypeBase entityType)
    {
        var typeMapping = property.GetTypeMapping();
        var type = typeMapping.ClrType.UnwrapEnumType();

        var generator = TryCreate();
        if (generator != null)
        {
            return generator;
        }

        type = typeMapping.Converter?.ProviderClrType.UnwrapEnumType();
        if (type != null)
        {
            generator = TryCreate();
            if (generator != null)
            {
                return generator;
            }
        }

        throw new ArgumentException(
            CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(TemporaryNumberValueGeneratorFactory), property.Name, property.DeclaringType.DisplayName()));

        ValueGenerator? TryCreate()
        {
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

            return null;
        }
    }
}
