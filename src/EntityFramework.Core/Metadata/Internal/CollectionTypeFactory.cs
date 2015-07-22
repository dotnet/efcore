// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class CollectionTypeFactory : ICollectionTypeFactory
    {
        public virtual Type TryFindTypeToInstantiate(Type collectionType)
        {
            // Code taken from EF6. The rules are:
            // If the collection is defined as a concrete type with a public parameterless constructor, then create an instance of that type
            // Else, if HashSet{T} can be assigned to the type, then use HashSet{T}
            // Else, if List{T} can be assigned to the type, then use List{T}
            // Else, return null.

            var elementType = collectionType.TryGetElementType(typeof(ICollection<>));

            if (elementType == null)
            {
                return null;
            }

            if (!collectionType.GetTypeInfo().IsAbstract)
            {
                var constructor = collectionType.GetDeclaredConstructor(null);
                if (constructor != null
                    && constructor.IsPublic)
                {
                    return collectionType;
                }
            }

            var hashSetOfT = typeof(HashSet<>).MakeGenericType(elementType);
            if (collectionType.GetTypeInfo().IsAssignableFrom(hashSetOfT.GetTypeInfo()))
            {
                return hashSetOfT;
            }

            var listOfT = typeof(List<>).MakeGenericType(elementType);
            if (collectionType.GetTypeInfo().IsAssignableFrom(listOfT.GetTypeInfo()))
            {
                return listOfT;
            }

            return null;
        }
    }
}
