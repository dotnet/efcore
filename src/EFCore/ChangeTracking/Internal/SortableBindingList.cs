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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SortableBindingList([NotNull] List<T> list)
            : base(list)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortProperty = null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool IsSortedCore => _isSorted;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override ListSortDirection SortDirectionCore => _sortDirection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore => _sortProperty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
