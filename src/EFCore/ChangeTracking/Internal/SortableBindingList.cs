// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SortableBindingList([NotNull] List<T> list)
            : base(list)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
        protected override bool IsSortedCore => _isSorted;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ListSortDirection SortDirectionCore => _sortDirection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore => _sortProperty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override bool SupportsSortingCore => true;

        private class PropertyComparer : Comparer<T>
        {
            private readonly IComparer _comparer;
            private readonly ListSortDirection _direction;
            private readonly PropertyDescriptor _prop;

            public PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
            {
                if (!prop.ComponentType.IsAssignableFrom(typeof(T)))
                {
                    throw new MissingMemberException(typeof(T).Name, prop.Name);
                }

                _prop = prop;
                _direction = direction;

                var property = typeof(Comparer<>).MakeGenericType(prop.PropertyType).GetTypeInfo().GetDeclaredProperty("Default");
                _comparer = (IComparer)property.GetValue(null, null);
            }

            public override int Compare(T left, T right)
            {
                var leftValue = _prop.GetValue(left);
                var rightValue = _prop.GetValue(right);

                return _direction == ListSortDirection.Ascending
                    ? _comparer.Compare(leftValue, rightValue)
                    : _comparer.Compare(rightValue, leftValue);
            }

            public static bool CanSort(Type type)
                => type.GetInterface("IComparable") != null
                    || (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
