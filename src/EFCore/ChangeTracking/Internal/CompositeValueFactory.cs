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
public class CompositeValueFactory : IDependentKeyValueFactory<object[]>
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
    public virtual IEqualityComparer<object[]> EqualityComparer { get; }

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
    public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, [NotNullWhen(true)] out object[]? key)
    {
        key = new object[Properties.Count];
        var index = 0;

        foreach (var property in Properties)
        {
            var value = valueBuffer[property.GetIndex()];
            if (value == null)
            {
                key = null;
                return false;
            }

            key[index++] = value;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out object[]? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out object[]? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetPreStoreGeneratedCurrentValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromOriginalValues(IUpdateEntry entry, [NotNullWhen(true)] out object[]? key)
        => TryCreateFromEntry(entry, (e, p) => e.GetOriginalValue(p), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, [NotNullWhen(true)] out object[]? key)
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
        [NotNullWhen(true)] out object[]? key)
    {
        key = new object[Properties.Count];
        var index = 0;

        foreach (var property in Properties)
        {
            var value = getValue(entry, property);
            if (value == null)
            {
                key = null;
                return false;
            }

            key[index++] = value;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static IEqualityComparer<object[]> CreateEqualityComparer(IReadOnlyList<IProperty> properties)
        => new CompositeCustomComparer(properties.Select(p => p.GetKeyValueComparer()).ToList());

    private sealed class CompositeCustomComparer : IEqualityComparer<object[]>
    {
        private readonly Func<object, object, bool>[] _equals;
        private readonly Func<object, int>[] _hashCodes;

        public CompositeCustomComparer(IList<ValueComparer> comparers)
        {
            _equals = comparers.Select(c => (Func<object, object, bool>)c.Equals).ToArray();
            _hashCodes = comparers.Select(c => (Func<object, int>)c.GetHashCode).ToArray();
        }

        public bool Equals(object[]? x, object[]? y)
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

            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!_equals[i](x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(object[] obj)
        {
            var hashCode = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < obj.Length; i++)
            {
                hashCode = (hashCode * 397) ^ _hashCodes[i](obj[i]);
            }

            return hashCode;
        }
    }
}
