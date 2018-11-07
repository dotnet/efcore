// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CollectionTypeFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type TryFindTypeToInstantiate([NotNull] Type entityType, [NotNull] Type collectionType)
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

            if (!collectionType.GetTypeInfo().IsAbstract)
            {
                var constructor = collectionType.GetDeclaredConstructor(null);
                if (constructor?.IsPublic == true)
                {
                    return collectionType;
                }
            }

            if (typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(entityType.GetTypeInfo()))
            {
                var observableHashSetOfT = typeof(ObservableHashSet<>).MakeGenericType(elementType);
                if (collectionType.GetTypeInfo().IsAssignableFrom(observableHashSetOfT.GetTypeInfo()))
                {
                    return observableHashSetOfT;
                }
            }

            var hashSetOfT = typeof(HashSet<>).MakeGenericType(elementType);
            if (collectionType.GetTypeInfo().IsAssignableFrom(hashSetOfT.GetTypeInfo()))
            {
                return hashSetOfT;
            }

            var listOfT = typeof(List<>).MakeGenericType(elementType);
            return collectionType.GetTypeInfo().IsAssignableFrom(listOfT.GetTypeInfo()) ? listOfT : null;
        }
    }
}
