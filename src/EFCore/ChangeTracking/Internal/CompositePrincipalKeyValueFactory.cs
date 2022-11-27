// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompositePrincipalKeyValueFactory : CompositeValueFactory, IPrincipalKeyValueFactory<IEnumerable<object?>>
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
    public virtual object? CreateFromKeyValues(IEnumerable<object?> keyValues)
        // ReSharper disable once PossibleMultipleEnumeration
        => keyValues.Any(v => v == null)
            ? null
            // ReSharper disable once PossibleMultipleEnumeration
            : keyValues;

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
    public virtual IProperty FindNullPropertyInKeyValues(object?[] keyValues)
    {
        var index = -1;
        for (var i = 0; i < keyValues.Length; i++)
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
    public virtual IEnumerable<object?> CreateFromCurrentValues(IUpdateEntry entry)
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
    public virtual IEnumerable<object?> CreateFromOriginalValues(IUpdateEntry entry)
        => CreateFromEntry(entry, (e, p) => e.GetOriginalValue(p));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<object?> CreateFromRelationshipSnapshot(IUpdateEntry entry)
        => CreateFromEntry(entry, (e, p) => e.GetRelationshipSnapshotValue(p));

    private object[] CreateFromEntry(
        IUpdateEntry entry,
        Func<IUpdateEntry, IProperty, object?> getValue)
    {
        var values = new object[Properties.Count];
        var index = 0;

        foreach (var property in Properties)
        {
            var value = getValue(entry, property);
            if (value == null)
            {
                return default!;
            }

            values[index++] = value;
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
        => new EquatableKeyValue<IEnumerable<object?>>(
            _key,
            fromOriginalValues
                ? CreateFromOriginalValues(entry)
                : CreateFromCurrentValues(entry),
            EqualityComparer);
}
