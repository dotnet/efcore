// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class RelationalJsonElement : IRelationalJsonElement
{
    private readonly List<IJsonElementMapping> _propertyMappings = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected RelationalJsonElement(
        IColumnBase containingColumn,
        bool isNullable)
    {
        ContainingColumn = containingColumn;
        Path = [];
        IsNullable = isNullable;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected RelationalJsonElement(
        string name,
        RelationalJsonObject parentElement,
        bool isNullable)
    {
        PropertyName = name;
        ContainingColumn = parentElement.ContainingColumn;
        Path = parentElement.CreateChildPath(name);
        ParentElement = parentElement;
        IsNullable = isNullable;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected RelationalJsonElement(
        RelationalJsonArray parentElement,
        bool isNullable)
    {
        ContainingColumn = parentElement.ContainingColumn;
        Path = [.. parentElement.Path, JsonPathSegment.Array];
        ParentElement = parentElement;
        IsNullable = isNullable;
    }

    /// <inheritdoc />
    public virtual string? PropertyName { get; }

    /// <inheritdoc />
    public virtual IColumnBase ContainingColumn { get; }

    /// <inheritdoc />
    public virtual RelationalTypeMapping? StoreTypeMapping
        => GetDefaultStoreTypeMapping();

    /// <inheritdoc />
    public virtual IReadOnlyList<JsonPathSegment> Path { get; protected set; }

    /// <inheritdoc />
    public virtual IRelationalJsonElement? ParentElement { get; }

    /// <inheritdoc />
    public virtual bool IsNullable { get; }

    /// <inheritdoc />
    public virtual IReadOnlyList<IJsonElementMapping> PropertyMappings
        => _propertyMappings;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? GetDefaultStoreTypeMapping()
    {
        if (PropertyMappings.Select(m => m.Property).OfType<IProperty>().FirstOrDefault()?.GetTypeMapping() is RelationalTypeMapping mapping)
        {
            return mapping;
        }

        return ParentElement is IRelationalJsonArray { StoreTypeMapping: { ElementTypeMapping: RelationalTypeMapping elementTypeMapping } }
            ? elementTypeMapping
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddPropertyMapping(IJsonElementMapping mapping)
        => _propertyMappings.Add(mapping);
}
