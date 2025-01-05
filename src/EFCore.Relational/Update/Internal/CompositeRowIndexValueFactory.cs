// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompositeRowIndexValueFactory : CompositeRowValueFactory, IRowIndexValueFactory<object?[]>
{
    private readonly ITableIndex _index;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositeRowIndexValueFactory(ITableIndex index)
        : base(index.Columns)
    {
        _index = index;

        EqualityComparer = CreateEqualityComparer(index.Columns, null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(
        object?[] keyValues,
        out object?[]? key,
        out bool hasNullValue)
        => TryCreateDependentKeyValue(keyValues, out key, out hasNullValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(
        IDictionary<string, object?> keyValues,
        out object?[]? key,
        out bool hasNullValue)
        => TryCreateDependentKeyValue(keyValues, out key, out hasNullValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues,
        out object?[]? keyValue,
        out bool hasNullValue)
        => TryCreateDependentKeyValue(command, fromOriginalValues, out keyValue, out hasNullValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (object? Value, bool HasNullValue) CreateEquatableIndexValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues = false)
        => TryCreateIndexValue(command, fromOriginalValues, out var keyValue, out var hasNullValue)
            ? (new EquatableKeyValue<object?[]>(_index, keyValue, EqualityComparer), hasNullValue)
            : (null, true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (object?[]? Value, bool HasNullValue) CreateIndexValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues = false)
        => TryCreateIndexValue(command, fromOriginalValues, out var keyValue, out var hasNullValue)
            ? (keyValue, hasNullValue)
            : (null, true);
}
