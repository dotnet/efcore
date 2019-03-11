// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for relational database metadata.
    /// </summary>
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     <para>
        ///         Checks whether or not the column mapped to the given <see cref="IProperty" /> will be nullable
        ///         or not when created in the database.
        ///     </para>
        ///     <para>
        ///         This can depend not just on the property itself, but also how it is mapped. For example,
        ///         non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <returns> <c>True</c> if the mapped column is nullable; <c>false</c> otherwise. </returns>
        public static bool IsColumnNullable([NotNull] this IProperty property)
            => property.DeclaringEntityType.BaseType != null
               || property.IsNullable ||
               !property.IsPrimaryKey()
               && IsOwnedByDerivedType(property.DeclaringEntityType);

        private static bool IsOwnedByDerivedType(IEntityType entityType)
        {
            var ownerEntityType = entityType.FindPrimaryKey()?.Properties.First()
                ?.FindSharedTableLink()?.PrincipalEntityType;

            return ownerEntityType?.BaseType != null ||
                   ownerEntityType != null
                   && IsOwnedByDerivedType(ownerEntityType);
        }

        /// <summary>
        ///     <para>
        ///         Finds the <see cref="IProperty"/> that represents the same primary key property
        ///         as the given property, but potentially in a shared root table.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The property found, or <code>null</code> if none was found.</returns>
        public static IProperty FindSharedTableRootPrimaryKeyProperty([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var principalProperty = property;
            HashSet<IEntityType> visitedTypes = null;
            while (true)
            {
                var linkingRelationship = principalProperty.FindSharedTableLink();
                if (linkingRelationship == null)
                {
                    break;
                }

                if (visitedTypes == null)
                {
                    visitedTypes = new HashSet<IEntityType>
                    {
                        linkingRelationship.DeclaringEntityType
                    };
                }

                if (!visitedTypes.Add(linkingRelationship.PrincipalEntityType))
                {
                    return null;
                }

                principalProperty = linkingRelationship.PrincipalKey.Properties[linkingRelationship.Properties.IndexOf(principalProperty)];
            }

            return principalProperty == property ? null : principalProperty;
        }

    }
}
