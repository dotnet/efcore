// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

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
    private readonly bool _shadow;
    private readonly Func<TEntity, TCollection>? _getCollection;
    private readonly Action<TEntity, TCollection>? _setCollection;
    private readonly Action<TEntity, TCollection>? _setCollectionForMaterialization;
    private readonly Func<TEntity, Action<TEntity, TCollection>, TCollection>? _createAndSetCollection;
    private readonly Func<TCollection>? _createCollection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type CollectionType
        => typeof(TCollection);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrICollectionAccessor(
        string propertyName,
        bool shadow,
        Func<TEntity, TCollection>? getCollection,
        Action<TEntity, TCollection>? setCollection,
        Action<TEntity, TCollection>? setCollectionForMaterialization,
        Func<TEntity, Action<TEntity, TCollection>, TCollection>? createAndSetCollection,
        Func<TCollection>? createCollection)
    {
        _propertyName = propertyName;
        _shadow = shadow;
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
        => AddStandalone(GetOrCreateCollection(entity, forMaterialization), value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool AddStandalone(object collection, object value)
    {
        if (!ContainsStandalone(collection, value))
        {
            ((ICollection<TElement>)collection).Add((TElement)value);

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

    private ICollection<TElement>? GetCollection(object instance)
    {
        if (_shadow)
        {
            // This method is only used when getting a collection from a not tracked or not-yet tracked entity,
            // which means there is never an existing collection.
            return (ICollection<TElement>?)_createCollection?.Invoke();
        }

        var enumerable = _getCollection!((TEntity)instance);
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
    public virtual bool ContainsStandalone(object collection, object value)
        => Contains((ICollection<TElement>)collection, value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool Remove(object entity, object value)
        => RemoveStandalone(GetCollection((TEntity)entity), value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool RemoveStandalone(object? collection, object value)
    {
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
                return sortedSet.TryGetValue((TElement)value, out var found)
                    && ReferenceEquals(found, value)
                    && sortedSet.Remove(found);
            default:
                return ((ICollection<TElement>?)collection)?.Remove((TElement)value) ?? false;
        }
    }

    private static bool Contains(ICollection<TElement>? collection, object value)
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
                return sortedSet.TryGetValue((TElement)value, out var found)
                    && ReferenceEquals(found, value);
            default:
                return collection?.Contains((TElement)value) == true;
        }
    }
}
