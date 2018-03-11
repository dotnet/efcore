// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SimplePrincipalKeyValueFactory<TKey> : IPrincipalKeyValueFactory<TKey>
    {
        private readonly IProperty _property;
        private readonly PropertyAccessors _propertyAccessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SimplePrincipalKeyValueFactory([NotNull] IProperty property)
        {
            _property = property;
            _propertyAccessors = _property.GetPropertyAccessors();

            var comparer = property.GetKeyValueComparer()
                           ?? property.FindMapping()?.KeyComparer;

            EqualityComparer
                = comparer != null
                    ? new NoNullsCustomEqualityComparer(comparer)
                    : typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(typeof(TKey).GetTypeInfo())
                        ? (IEqualityComparer<TKey>)new NoNullsStructuralEqualityComparer()
                        : EqualityComparer<TKey>.Default;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateFromKeyValues(object[] keyValues)
            => keyValues[0];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateFromBuffer(ValueBuffer valueBuffer)
            => _propertyAccessors.ValueBufferGetter(valueBuffer);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IProperty FindNullPropertyInValueBuffer(ValueBuffer valueBuffer)
            => _property;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TKey CreateFromCurrentValues(InternalEntityEntry entry)
            => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.CurrentValueGetter)(entry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IProperty FindNullPropertyInCurrentValues(InternalEntityEntry entry)
            => _property;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TKey CreateFromOriginalValues(InternalEntityEntry entry)
            => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.OriginalValueGetter)(entry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TKey CreateFromRelationshipSnapshot(InternalEntityEntry entry)
            => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)(entry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEqualityComparer<TKey> EqualityComparer { get; }

        private sealed class NoNullsStructuralEqualityComparer : IEqualityComparer<TKey>
        {
            private readonly IEqualityComparer _comparer
                = StructuralComparisons.StructuralEqualityComparer;

            public bool Equals(TKey x, TKey y) => _comparer.Equals(x, y);
            public int GetHashCode(TKey obj) => _comparer.GetHashCode(obj);
        }

        private sealed class NoNullsCustomEqualityComparer : IEqualityComparer<TKey>
        {
            private readonly Func<TKey, TKey, bool> _equals;
            private readonly Func<TKey, int> _hashCode;

            public NoNullsCustomEqualityComparer(ValueComparer comparer)
            {
                if (comparer.Type != typeof(TKey)
                    && comparer.Type == typeof(TKey).UnwrapNullableType())
                {
                    comparer = comparer.ToNonNullNullableComparer();
                }

                _equals = (Func<TKey, TKey, bool>)comparer.EqualsExpression.Compile();
                _hashCode = (Func<TKey, int>)comparer.HashCodeExpression.Compile();
            }

            public bool Equals(TKey x, TKey y) => _equals(x, y);

            public int GetHashCode(TKey obj) => _hashCode(obj);
        }
    }
}
