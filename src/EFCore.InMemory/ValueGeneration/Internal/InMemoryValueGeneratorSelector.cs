// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryValueGeneratorSelector : ValueGeneratorSelector
{
    private readonly IInMemoryStore _inMemoryStore;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        IInMemoryDatabase inMemoryDatabase)
        : base(dependencies)
    {
        _inMemoryStore = inMemoryDatabase.Store;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ValueGenerator Select(IProperty property, IEntityType entityType)
        => property.GetValueGeneratorFactory() == null
            && property.ClrType.IsInteger()
            && property.ClrType.UnwrapNullableType() != typeof(char)
                ? GetOrCreate(property)
                : base.Select(property, entityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private ValueGenerator GetOrCreate(IProperty property)
    {
        var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();
        if (FindGenerator(property, type, out var valueGenerator))
        {
            return valueGenerator!;
        }

        throw new ArgumentException(
            CoreStrings.InvalidValueGeneratorFactoryProperty(
                "InMemoryIntegerValueGeneratorFactory", property.Name, property.DeclaringEntityType.DisplayName()));
    }

    private bool FindGenerator(IProperty property, Type type, out ValueGenerator? valueGenerator)
    {
        if (type == typeof(long))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<long>(property);
                return true;
            }
        }

        if (type == typeof(int))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<int>(property);
                return true;
            }
        }

        if (type == typeof(short))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<short>(property);
                return true;
            }
        }

        if (type == typeof(byte))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<byte>(property);
                return true;
            }
        }

        if (type == typeof(ulong))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<ulong>(property);
                return true;
            }
        }

        if (type == typeof(uint))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<uint>(property);
                return true;
            }
        }

        if (type == typeof(ushort))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<ushort>(property);
                return true;
            }
        }

        if (type == typeof(sbyte))
        {
            {
                valueGenerator = _inMemoryStore.GetIntegerValueGenerator<sbyte>(property);
                return true;
            }
        }

        valueGenerator = null;
        return false;
    }

    /// <inheritdoc />
    protected override ValueGenerator? FindForType(IProperty property, IEntityType entityType, Type clrType)
        => property.ValueGenerated != ValueGenerated.Never && FindGenerator(property, clrType, out var valueGenerator)
            ? valueGenerator!
            : base.FindForType(property, entityType, clrType);
}
