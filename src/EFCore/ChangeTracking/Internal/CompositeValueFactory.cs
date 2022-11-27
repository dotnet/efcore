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
public class CompositeValueFactory : IDependentKeyValueFactory<IEnumerable<object?>>
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
    public virtual IEqualityComparer<IEnumerable<object?>> EqualityComparer { get; }

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
    public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, [NotNullWhen(true)] out IEnumerable<object?>? key)
    {
        var keyArray = new object[Properties.Count];
        var index = 0;

        foreach (var property in Properties)
        {
            var value = valueBuffer[property.GetIndex()];
            if (value == null)
            {
                key = null;
                return false;
            }

            keyArray[index++] = value;
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
    public virtual bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out IEnumerable<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out IEnumerable<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetPreStoreGeneratedCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromOriginalValues(IUpdateEntry entry, [NotNullWhen(true)] out IEnumerable<object?>? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetOriginalValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, [NotNullWhen(true)] out IEnumerable<object?>? key)
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
        [NotNullWhen(true)] out IEnumerable<object?>? key)
    {
        var keyArray = new object[Properties.Count];
        var index = 0;

        foreach (var property in Properties)
        {
            var value = getValue(entry, property);
            if (value == null)
            {
                key = null;
                return false;
            }

            keyArray[index++] = value;
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
    protected static IEqualityComparer<IEnumerable<object?>> CreateEqualityComparer(IReadOnlyList<IProperty> properties)
        => new CompositeCustomComparer(properties.Select(p => p.GetKeyValueComparer()).ToList());

    private sealed class CompositeCustomComparer : IEqualityComparer<IEnumerable<object?>>
    {
        private readonly int _valueCount;
        private readonly Func<object, object, bool>[] _equals;
        private readonly Func<object, int>[] _hashCodes;

        public CompositeCustomComparer(IList<ValueComparer> comparers)
        {
            _valueCount = comparers.Count;
            _equals = comparers.Select(c => (Func<object, object, bool>)c.Equals).ToArray();
            _hashCodes = comparers.Select(c => (Func<object, int>)c.GetHashCode).ToArray();
        }

        public bool Equals(IEnumerable<object?>? x, IEnumerable<object?>? y)
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

            if (x is IReadOnlyList<object> xList
                && y is IReadOnlyList<object> yList)
            {
                if (xList.Count != _valueCount
                    || yList.Count != _valueCount)
                {
                    return false;
                }

                for (var i = 0; i < _valueCount; i++)
                {
                    if (!_equals[i](xList[i], yList[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                using var xEnumerator = x.GetEnumerator();
                using var yEnumerator = y.GetEnumerator();

                for (var i = 0; i < _valueCount; i++)
                {
                    if (!xEnumerator.MoveNext()
                        || !yEnumerator.MoveNext())
                    {
                        return false;
                    }

                    if (!_equals[i++](xEnumerator.Current!, yEnumerator.Current!))
                    {
                        return false;
                    }
                }

                if (xEnumerator.MoveNext()
                    || yEnumerator.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IEnumerable<object?> obj)
        {
            var hashCode = new HashCode();

            using var enumerator = obj.GetEnumerator();
            for (var i = 0; i < _valueCount && enumerator.MoveNext(); i++)
            {
                hashCode.Add(_hashCodes[i](enumerator.Current!));
            }

            return hashCode.ToHashCode();
        }
    }
}
