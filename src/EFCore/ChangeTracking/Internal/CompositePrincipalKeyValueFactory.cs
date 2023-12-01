// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompositePrincipalKeyValueFactory : CompositeValueFactory, IPrincipalKeyValueFactory<IReadOnlyList<object?>>
{
    private readonly IKey _key;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositePrincipalKeyValueFactory(IKey key)
        : base(key.Properties)
    {
        _key = key;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateFromKeyValues(IReadOnlyList<object?> keyValues) // ReSharper disable once PossibleMultipleEnumeration
    {
        for (var i = 0; i < keyValues.Count; i++)
        {
            if (keyValues[i] == null)
            {
                return null;
            }
        }

        return keyValues;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateFromBuffer(ValueBuffer valueBuffer)
        => TryCreateFromBuffer(valueBuffer, out var values) ? values : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInKeyValues(IReadOnlyList<object?> keyValues)
    {
        var index = -1;
        for (var i = 0; i < keyValues.Count; i++)
        {
            if (keyValues[i] == null)
            {
                index = i;
                break;
            }
        }

        return Properties[index];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<object?> CreateFromCurrentValues(IUpdateEntry entry)
        => CreateFromEntry(entry, (e, p) => e.GetCurrentValue(p));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty? FindNullPropertyInCurrentValues(IUpdateEntry entry)
        => Properties.FirstOrDefault(p => entry.GetCurrentValue(p) == null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<object?> CreateFromOriginalValues(IUpdateEntry entry)
        => CreateFromEntry(entry, (e, p) => e.GetOriginalOrCurrentValue(p));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<object?> CreateFromRelationshipSnapshot(IUpdateEntry entry)
        => CreateFromEntry(entry, (e, p) => e.GetRelationshipSnapshotValue(p));

    private object[] CreateFromEntry(
        IUpdateEntry entry,
        Func<IUpdateEntry, IProperty, object?> getValue)
    {
        var values = new object[Properties.Count];
        for (var i = 0; i < values.Length; i++)
        {
            var value = getValue(entry, Properties[i]);
            if (value == null)
            {
                return default!;
            }

            values[i] = value;
        }

        return values;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreateEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => new EquatableKeyValue<IReadOnlyList<object?>>(
            _key,
            fromOriginalValues
                ? CreateFromOriginalValues(entry)
                : CreateFromCurrentValues(entry),
            EqualityComparer);
}
