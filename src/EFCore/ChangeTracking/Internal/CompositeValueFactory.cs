// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompositeValueFactory : IDependentKeyValueFactory<IReadOnlyList<object?>>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositeValueFactory(IReadOnlyList<IProperty> properties)
    {
        Properties = properties;
        EqualityComparer = CreateEqualityComparer(properties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEqualityComparer<IReadOnlyList<object?>> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IReadOnlyList<IProperty> Properties { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, [NotNullWhen(true)] out IReadOnlyList<object?>? key)
    {
        var keyArray = new object[Properties.Count];
        for (var i = 0; i < keyArray.Length; i++)
        {
            var value = valueBuffer[Properties[i].GetIndex()];
            if (value == null)
            {
                key = null;
                return false;
            }

            keyArray[i] = value;
        }

        key = keyArray;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out IReadOnlyList<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out IReadOnlyList<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetPreStoreGeneratedCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromOriginalValues(IUpdateEntry entry, [NotNullWhen(true)] out IReadOnlyList<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetOriginalOrCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, [NotNullWhen(true)] out IReadOnlyList<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetRelationshipSnapshotValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool TryCreateFromEntry(
        IUpdateEntry entry,
        Func<IUpdateEntry, IProperty, object?> getValue,
        [NotNullWhen(true)] out IReadOnlyList<object?>? key)
    {
        var keyArray = new object[Properties.Count];
        for (var i = 0; i < keyArray.Length; i++)
        {
            var value = getValue(entry, Properties[i]);
            if (value == null)
            {
                key = null;
                return false;
            }

            keyArray[i] = value;
        }

        key = keyArray;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreatePrincipalEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => throw new NotImplementedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateDependentEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => throw new NotImplementedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static IEqualityComparer<IReadOnlyList<object?>> CreateEqualityComparer(IReadOnlyList<IProperty> properties)
        => new CompositeCustomComparer(properties.Select(p => p.GetKeyValueComparer()).ToArray());

    private sealed class CompositeCustomComparer : IEqualityComparer<IReadOnlyList<object?>>
    {
        private readonly ValueComparer[] _comparers;

        public CompositeCustomComparer(ValueComparer[] comparers)
        {
            _comparers = comparers;
        }

        public bool Equals(IReadOnlyList<object?>? x, IReadOnlyList<object?>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return y is null;
            }

            if (y is null)
            {
                return false;
            }

            if (x.Count != _comparers.Length
                || y.Count != _comparers.Length)
            {
                return false;
            }

            for (var i = 0; i < _comparers.Length; i++)
            {
                if (!_comparers[i].Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IReadOnlyList<object?> obj)
        {
            var hashCode = new HashCode();

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < obj.Count; i++)
            {
                hashCode.Add(_comparers[i].GetHashCode(obj[i]));
            }

            return hashCode.ToHashCode();
        }
    }
}
