// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     <para>
///         Selects value generators to be used to generate values for properties of entities.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class RelationalValueGeneratorSelector : ValueGeneratorSelector
{
    private readonly TemporaryNumberValueGeneratorFactory _numberFactory
        = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalValueGeneratorSelector" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public RelationalValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
    {
        if (typeBase.IsMappedToJson() && property.IsOrdinalKeyProperty())
        {
            return _numberFactory.Create(property, typeBase);
        }

        if (property.ValueGenerated != ValueGenerated.Never)
        {
            if (clrType.IsInteger()
                || clrType == typeof(decimal)
                || clrType == typeof(float)
                || clrType == typeof(double))
            {
                return _numberFactory.Create(property, typeBase);
            }

            if (clrType == typeof(DateTime))
            {
                return new TemporaryDateTimeValueGenerator();
            }

            if (clrType == typeof(DateTimeOffset))
            {
                return new TemporaryDateTimeOffsetValueGenerator();
            }

            if (property.GetDefaultValueSql() != null)
            {
                if (clrType == typeof(Guid))
                {
                    return new TemporaryGuidValueGenerator();
                }

                if (clrType == typeof(string))
                {
                    return new TemporaryStringValueGenerator();
                }

                if (clrType == typeof(byte[]))
                {
                    return new TemporaryBinaryValueGenerator();
                }
            }
        }

        return base.FindForType(property, typeBase, clrType);
    }
}
