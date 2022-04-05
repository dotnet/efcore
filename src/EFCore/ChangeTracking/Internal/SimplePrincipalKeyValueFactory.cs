// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimplePrincipalKeyValueFactory<TKey> : IPrincipalKeyValueFactory<TKey>
{
    private readonly IProperty _property;
    private readonly PropertyAccessors _propertyAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimplePrincipalKeyValueFactory(IProperty property)
    {
        _property = property;
        _propertyAccessors = _property.GetPropertyAccessors();

        EqualityComparer = new NoNullsCustomEqualityComparer(property.GetKeyValueComparer());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateFromKeyValues(object?[] keyValues)
        => keyValues[0];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateFromBuffer(ValueBuffer valueBuffer)
        => _propertyAccessors.ValueBufferGetter!(valueBuffer);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInKeyValues(object?[] keyValues)
        => _property;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromCurrentValues(IUpdateEntry entry)
        => ((Func<IUpdateEntry, TKey>)_propertyAccessors.CurrentValueGetter)(entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInCurrentValues(IUpdateEntry entry)
        => _property;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromOriginalValues(IUpdateEntry entry)
        => ((Func<IUpdateEntry, TKey>)_propertyAccessors.OriginalValueGetter!)(entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromRelationshipSnapshot(IUpdateEntry entry)
        => ((Func<IUpdateEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)(entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEqualityComparer<TKey> EqualityComparer { get; }

    private sealed class NoNullsStructuralEqualityComparer : IEqualityComparer<TKey>
    {
        private readonly IEqualityComparer _comparer
            = StructuralComparisons.StructuralEqualityComparer;

        public bool Equals(TKey? x, TKey? y)
            => _comparer.Equals(x, y);

        public int GetHashCode([DisallowNull] TKey obj)
            => _comparer.GetHashCode(obj);
    }

    private sealed class NoNullsCustomEqualityComparer : IEqualityComparer<TKey>
    {
        private readonly Func<TKey?, TKey?, bool> _equals;
        private readonly Func<TKey, int> _hashCode;

        public NoNullsCustomEqualityComparer(ValueComparer comparer)
        {
            if (comparer.Type != typeof(TKey)
                && comparer.Type == typeof(TKey).UnwrapNullableType())
            {
                comparer = comparer.ToNonNullNullableComparer();
            }

            _equals = (Func<TKey?, TKey?, bool>)comparer.EqualsExpression.Compile();
            _hashCode = (Func<TKey, int>)comparer.HashCodeExpression.Compile();
        }

        public bool Equals(TKey? x, TKey? y)
            => _equals(x, y);

        public int GetHashCode([DisallowNull] TKey obj)
            => _hashCode(obj);
    }
}
