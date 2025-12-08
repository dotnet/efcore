// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CollectionTypeFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CollectionTypeFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly CollectionTypeFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? TryFindTypeToInstantiate(
        Type entityType,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
            | DynamicallyAccessedMemberTypes.Interfaces)]
        Type collectionType,
        bool requireFullNotifications)
    {
        // Code taken from EF6. The rules are:
        // If the collection is defined as a concrete type with a public parameterless constructor, then create an instance of that type
        // Else, if entity type is notifying and ObservableHashSet{T} can be assigned to the type, then use ObservableHashSet{T}
        // Else, if HashSet{T} can be assigned to the type, then use HashSet{T}
        // Else, if List{T} can be assigned to the type, then use List{T}
        // Else, return null.

        var elementType = collectionType.TryGetElementType(typeof(IEnumerable<>));
        if (elementType == null)
        {
            return null;
        }

        if (!collectionType.IsAbstract)
        {
            var constructor = collectionType.GetDeclaredConstructor(null);
            if (constructor?.IsPublic == true)
            {
                return collectionType;
            }
        }

        if (requireFullNotifications
            || typeof(INotifyPropertyChanged).IsAssignableFrom(entityType))
        {
            var observableHashSetOfT = typeof(ObservableHashSet<>).MakeGenericType(elementType);
            if (collectionType.IsAssignableFrom(observableHashSetOfT))
            {
                return observableHashSetOfT;
            }
        }

        var hashSetOfT = typeof(HashSet<>).MakeGenericType(elementType);
        if (collectionType.IsAssignableFrom(hashSetOfT))
        {
            return hashSetOfT;
        }

        var listOfT = typeof(List<>).MakeGenericType(elementType);
        return collectionType.IsAssignableFrom(listOfT) ? listOfT : null;
    }
}
