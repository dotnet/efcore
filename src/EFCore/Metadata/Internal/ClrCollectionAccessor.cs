// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ClrICollectionAccessor<TEntity, TCollection, TElement> : IClrCollectionAccessor
        where TEntity : class
        where TCollection : class, IEnumerable<TElement>
        where TElement : class
    {
        private readonly string _propertyName;
        private readonly Func<TEntity, TCollection> _getCollection;
        private readonly Action<TEntity, TCollection> _setCollection;
        private readonly Action<TEntity, TCollection> _setCollectionForMaterialization;
        private readonly Func<TEntity, Action<TEntity, TCollection>, TCollection> _createAndSetCollection;
        private readonly Func<TCollection> _createCollection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type CollectionType => typeof(TCollection);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ClrICollectionAccessor(
            [NotNull] string propertyName,
            [NotNull] Func<TEntity, TCollection> getCollection,
            [CanBeNull] Action<TEntity, TCollection> setCollection,
            [CanBeNull] Action<TEntity, TCollection> setCollectionForMaterialization,
            [CanBeNull] Func<TEntity, Action<TEntity, TCollection>, TCollection> createAndSetCollection,
            [CanBeNull] Func<TCollection> createCollection)
        {
            _propertyName = propertyName;
            _getCollection = getCollection;
            _setCollection = setCollection;
            _setCollectionForMaterialization = setCollectionForMaterialization;
            _createAndSetCollection = createAndSetCollection;
            _createCollection = createCollection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Add(object entity, object value, bool forMaterialization)
        {
            var collection = GetOrCreateCollection(entity, forMaterialization);
            var element = (TElement)value;

            if (!Contains(collection, value))
            {
                collection.Add(element);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object Create()
        {
            if (_createCollection == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationCannotCreateType(
                        _propertyName, typeof(TEntity).ShortDisplayName(), typeof(TCollection).ShortDisplayName()));
            }

            return _createCollection();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object GetOrCreate(object entity, bool forMaterialization)
            => GetOrCreateCollection(entity, forMaterialization);

        private ICollection<TElement> GetOrCreateCollection(object instance, bool forMaterialization)
        {
            var collection = GetCollection(instance);

            if (collection == null)
            {
                var setCollection = forMaterialization
                    ? _setCollectionForMaterialization
                    : _setCollection;

                if (setCollection == null)
                {
                    throw new InvalidOperationException(CoreStrings.NavigationNoSetter(_propertyName, typeof(TEntity).ShortDisplayName()));
                }

                if (_createAndSetCollection == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationCannotCreateType(
                            _propertyName, typeof(TEntity).ShortDisplayName(), typeof(TCollection).ShortDisplayName()));
                }

                collection = (ICollection<TElement>)_createAndSetCollection((TEntity)instance, setCollection);
            }

            return collection;
        }

        private ICollection<TElement> GetCollection(object instance)
        {
            var enumerable = _getCollection((TEntity)instance);
            var collection = enumerable as ICollection<TElement>;

            if (enumerable != null
                && collection == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationBadType(
                        _propertyName,
                        typeof(TEntity).ShortDisplayName(),
                        enumerable.GetType().ShortDisplayName(),
                        typeof(TElement).ShortDisplayName()));
            }

            return collection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Contains(object entity, object value)
            => Contains(GetCollection((TEntity)entity), value);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Remove(object entity, object value)
        {
            var collection = GetCollection((TEntity)entity);

            switch (collection)
            {
                case List<TElement> list:
                    for (var i = 0; i < list.Count; i++)
                    {
                        if (ReferenceEquals(list[i], value))
                        {
                            list.RemoveAt(i);
                            return true;
                        }
                    }

                    return false;
                case Collection<TElement> concreteCollection:
                    for (var i = 0; i < concreteCollection.Count; i++)
                    {
                        if (ReferenceEquals(concreteCollection[i], value))
                        {
                            concreteCollection.RemoveAt(i);
                            return true;
                        }
                    }

                    return false;
                case SortedSet<TElement> sortedSet:
                    foreach (var item in sortedSet)
                    {
                        if (ReferenceEquals(item, value))
                        {
                            sortedSet.Remove(item);
                            return true;
                        }
                    }

                    return false;
                default:
                    return collection?.Remove((TElement)value) ?? false;
            }
        }

        private static bool Contains(ICollection<TElement> collection, object value)
        {
            switch (collection)
            {
                case List<TElement> list:
                    foreach (var element in list)
                    {
                        if (ReferenceEquals(element, value))
                        {
                            return true;
                        }
                    }

                    return false;
                case Collection<TElement> concreteCollection:
                    for (var i = 0; i < concreteCollection.Count; i++)
                    {
                        if (ReferenceEquals(concreteCollection[i], value))
                        {
                            return true;
                        }
                    }

                    return false;
                case SortedSet<TElement> sortedSet:
                    foreach (var element in sortedSet)
                    {
                        if (ReferenceEquals(element, value))
                        {
                            return true;
                        }
                    }

                    return false;
                default:
                    return collection?.Contains((TElement)value) == true;
            }
        }
    }
}
