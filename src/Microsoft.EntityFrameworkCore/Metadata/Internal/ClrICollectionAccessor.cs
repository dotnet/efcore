// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ClrICollectionAccessor<TEntity, TCollection, TElement> : IClrCollectionAccessor
        where TEntity : class
        where TCollection : class, ICollection<TElement>
    {
        private readonly string _propertyName;
        private readonly Func<TEntity, TCollection> _getCollection;
        private readonly Action<TEntity, TCollection> _setCollection;
        private readonly Func<TEntity, Action<TEntity, TCollection>, TCollection> _createAndSetCollection;
        private readonly Func<TCollection> _createCollection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type CollectionType => typeof(TCollection);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ClrICollectionAccessor(
            [NotNull] string propertyName,
            [NotNull] Func<TEntity, TCollection> getCollection,
            [CanBeNull] Action<TEntity, TCollection> setCollection,
            [CanBeNull] Func<TEntity, Action<TEntity, TCollection>, TCollection> createAndSetCollection,
            [CanBeNull] Func<TCollection> createCollection)
        {
            _propertyName = propertyName;
            _getCollection = getCollection;
            _setCollection = setCollection;
            _createAndSetCollection = createAndSetCollection;
            _createCollection = createCollection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Add(object instance, object value)
        {
            var collection = GetOrCreateCollection(instance);
            var element = (TElement)value;

            if (!collection.Contains(element))
            {
                collection.Add(element);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddRange(object instance, IEnumerable<object> values)
        {
            var collection = GetOrCreateCollection(instance);

            foreach (TElement value in values)
            {
                if (!collection.Contains(value))
                {
                    collection.Add(value);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Create(IEnumerable<object> values)
        {
            if (_createCollection == null)
            {
                throw new InvalidOperationException(CoreStrings.NavigationCannotCreateType(
                    _propertyName, typeof(TEntity).FullName, typeof(TCollection).FullName));
            }

            var collection = _createCollection();
            foreach (TElement value in values)
            {
                collection.Add(value);
            }

            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetOrCreate(object instance) => GetOrCreateCollection(instance);

        private TCollection GetOrCreateCollection(object instance)
        {
            var collection = _getCollection((TEntity)instance);

            if (collection == null)
            {
                if (_setCollection == null)
                {
                    throw new InvalidOperationException(CoreStrings.NavigationNoSetter(_propertyName, typeof(TEntity).FullName));
                }

                if (_createAndSetCollection == null)
                {
                    throw new InvalidOperationException(CoreStrings.NavigationCannotCreateType(
                        _propertyName, typeof(TEntity).FullName, typeof(TCollection).FullName));
                }

                collection = _createAndSetCollection((TEntity)instance, _setCollection);
            }
            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Contains(object instance, object value)
        {
            var collection = _getCollection((TEntity)instance);

            return (collection != null) && collection.Contains((TElement)value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Remove(object instance, object value)
            => _getCollection((TEntity)instance)?.Remove((TElement)value);
    }
}
