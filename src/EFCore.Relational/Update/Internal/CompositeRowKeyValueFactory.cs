// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompositeRowKeyValueFactory : CompositeRowValueFactory, IRowKeyValueFactory<object?[]>
{
    private readonly IUniqueConstraint _constraint;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositeRowKeyValueFactory(IUniqueConstraint key)
        : base(key.Columns)
    {
        _constraint = key;

        EqualityComparer = CreateEqualityComparer(key.Columns, null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object?[] CreateKeyValue(object?[] keyValues)
    {
        if (keyValues.Any(v => v == null))
        {
            throw new InvalidOperationException(
                RelationalStrings.NullKeyValue(
                    _constraint.Table.SchemaQualifiedName,
                    FindNullColumnInKeyValues(keyValues).Name));
        }

        return keyValues;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object?[] CreateKeyValue(IDictionary<string, object?> keyValues)
    {
        if (!TryCreateDependentKeyValue(keyValues, out var key))
        {
            throw new InvalidOperationException(
                RelationalStrings.NullKeyValue(
                    _constraint.Table.SchemaQualifiedName,
                    FindNullColumnInKeyValues(key).Name));
        }

        return key;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object?[] CreateKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
    {
        if (!TryCreateDependentKeyValue(command, fromOriginalValues, out var keyValue))
        {
            throw new InvalidOperationException(
                RelationalStrings.NullKeyValue(
                    _constraint.Table.SchemaQualifiedName,
                    FindNullColumnInKeyValues(keyValue).Name));
        }

        return keyValue;
    }

    private IColumn FindNullColumnInKeyValues(object?[]? keyValues)
    {
        var index = 0;
        if (keyValues != null)
        {
            for (var i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i] == null)
                {
                    index = i;
                    break;
                }
            }
        }

        return _constraint.Columns[index];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreateEquatableKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => new EquatableKeyValue<object?[]>(
            _constraint,
            CreateKeyValue(command, fromOriginalValues),
            EqualityComparer);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    object[] IRowKeyValueFactory.CreateKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues)
        => CreateKeyValue(command, fromOriginalValues)!;
}
