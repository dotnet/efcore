// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalJsonArray : RelationalJsonElement, IRelationalJsonArray
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalJsonArray(
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
    public RelationalJsonArray(
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
    public RelationalJsonArray(
        RelationalJsonArray parentElement,
        bool isNullable)
        : base(parentElement, isNullable)
    {
    }

    /// <inheritdoc />
    public virtual IRelationalJsonElement ElementType
    {
        get => field;
        set
        {
            Check.DebugAssert(field == null, $"ElementType has already been set to {field}.");
            Check.DebugAssert(value == null || value.ParentElement == this, $"ElementType's parent must be this JSON array, not {value!.ParentElement}.");

            field = value!;
        }
    } = null!;
}
