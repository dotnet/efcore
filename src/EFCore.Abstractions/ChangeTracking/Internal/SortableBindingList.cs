// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[RequiresUnreferencedCode("Raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
[RequiresDynamicCode("Requires calling MakeGenericType on the property descriptor's type")]
public class SortableBindingList<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : BindingList<T>
{
    private bool _isSorted;
    private ListSortDirection _sortDirection;
    private PropertyDescriptor? _sortProperty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode("Raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
    public SortableBindingList(List<T> list)
        : base(list)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode("Requires accessing property 'Default' on the property descriptor's type")]
    [RequiresDynamicCode("Requires calling MakeGenericType on the property descriptor's type")]
    [UnconditionalSuppressMessage(
        "ReflectionAnalysis",
        "IL2046",
        Justification =
            "This method is an override, and the base method isn't annotated with RequiresUnreferencedCode. "
            + "The entire type is marked with RequiresUnreferencedCode.")]
    [SuppressMessage(
        "AOT", "IL3051:'RequiresDynamicCodeAttribute' annotations must match across all interface implementations or overrides.",
        Justification = "This method is an override, and the base method isn't annotated with RequiresDynamicCode. "
            + "The entire type is marked with RequiresDynamicCode.")]
    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
        if (PropertyComparer.CanSort(prop.PropertyType))
        {
            ((List<T>)Items).Sort(new PropertyComparer(prop, direction));
            _sortDirection = direction;
            _sortProperty = prop;
            _isSorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void RemoveSortCore()
    {
        _isSorted = false;
        _sortProperty = null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsSortedCore
        => _isSorted;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ListSortDirection SortDirectionCore
        => _sortDirection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override PropertyDescriptor? SortPropertyCore
        => _sortProperty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool SupportsSortingCore
        => true;

    private sealed class PropertyComparer : Comparer<T>
    {
        private readonly IComparer _comparer;
        private readonly ListSortDirection _direction;
        private readonly PropertyDescriptor _prop;

        [RequiresUnreferencedCode("Requires accessing property 'Default' on the property descriptor's type")]
        [RequiresDynamicCode("Requires calling MakeGenericType on the property descriptor's type")]
        public PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (!prop.ComponentType.IsAssignableFrom(typeof(T)))
            {
                throw new MissingMemberException(typeof(T).Name, prop.Name);
            }

            _prop = prop;
            _direction = direction;

            var property = typeof(Comparer<>).MakeGenericType(prop.PropertyType).GetTypeInfo().GetDeclaredProperty("Default")!;
            _comparer = (IComparer)property.GetValue(null, null)!;
        }

        public override int Compare(T? left, T? right)
        {
            if (left is null)
            {
                return right is null ? 0 : -1;
            }

            if (right is null)
            {
                return 1;
            }

            var leftValue = _prop.GetValue(left);
            var rightValue = _prop.GetValue(right);

            return _direction == ListSortDirection.Ascending
                ? _comparer.Compare(leftValue, rightValue)
                : _comparer.Compare(rightValue, leftValue);
        }

        public static bool CanSort([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
            => type.GetInterface("IComparable") != null
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }
}
