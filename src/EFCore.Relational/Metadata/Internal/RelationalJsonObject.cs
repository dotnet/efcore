// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalJsonObject : RelationalJsonElement, IRelationalJsonObject
{
    private readonly Utilities.OrderedDictionary<string, IRelationalJsonElement> _properties = new(StringComparer.Ordinal);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalJsonObject(
        IColumnBase containingColumn,
        bool isNullable)
        : base(containingColumn, isNullable)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalJsonObject(
        string name,
        RelationalJsonObject parentElement,
        bool isNullable)
        : base(name, parentElement, isNullable)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalJsonObject(
        RelationalJsonArray parentElement,
        bool isNullable)
        : base(parentElement, isNullable)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<JsonPathSegment> CreateChildPath(string childName)
    {
        var path = new JsonPathSegment[Path.Count + 1];
        for (var i = 0; i < Path.Count; i++)
        {
            path[i] = Path[i];
        }

        path[^1] = new JsonPathSegment(childName);
        return path;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<IRelationalJsonElement> Properties
        => _properties.Values;

    /// <inheritdoc />
    public virtual IRelationalJsonElement? FindProperty(string name)
        => _properties.GetValueOrDefault(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddProperty(IRelationalJsonElement property)
    {
        Check.DebugAssert(property.PropertyName != null, "Property added to a JSON object must have a name.");
        Check.DebugAssert(property.ParentElement == this, "Property's parent must be this JSON object.");

        _properties[property.PropertyName] = property;
    }
}
