// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimpleRowKeyValueFactory<TKey> : IRowKeyValueFactory<TKey>
{
    private readonly IUniqueConstraint _constraint;
    private readonly IColumn _column;
    private readonly ColumnAccessors _columnAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleRowKeyValueFactory(IUniqueConstraint constraint)
    {
        _constraint = constraint;
        _column = constraint.Columns.Single();
        _columnAccessors = ((Column)_column).Accessors;
        EqualityComparer = new NoNullsCustomEqualityComparer(_column.PropertyMappings.First().TypeMapping.ProviderComparer);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEqualityComparer<TKey> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateKeyValue(object?[] keyValues)
    {
        var value = (TKey?)keyValues[0];
        if (value == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.NullKeyValue(
                    _constraint.Table.SchemaQualifiedName,
                    _column.Name));
        }

        return value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateKeyValue(IDictionary<string, object?> keyValues)
    {
        var value = (TKey?)keyValues[_column.Name];
        if (value == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.NullKeyValue(
                    _constraint.Table.SchemaQualifiedName,
                    _column.Name));
        }

        return value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
    {
        var (key, found) = fromOriginalValues
            ? ((Func<IReadOnlyModificationCommand, (TKey, bool)>)_columnAccessors.OriginalValueGetter)(command)
            : ((Func<IReadOnlyModificationCommand, (TKey, bool)>)_columnAccessors.CurrentValueGetter)(command);

        if (!found)
        {
            throw new InvalidOperationException(
                RelationalStrings.NullKeyValue(
                    _constraint.Table.SchemaQualifiedName,
                    _column.Name));
        }

        return key;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreateValueIndex(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => new ValueIndex<TKey>(
            _constraint,
            CreateKeyValue(command, fromOriginalValues),
            EqualityComparer);

    object[] IRowKeyValueFactory.CreateKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues)
        => new object[] { CreateKeyValue(command, fromOriginalValues)! };

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
#pragma warning disable EF1001 // Internal EF Core API usage.
                comparer = comparer.ToNonNullNullableComparer();
#pragma warning restore EF1001 // Internal EF Core API usage.
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
