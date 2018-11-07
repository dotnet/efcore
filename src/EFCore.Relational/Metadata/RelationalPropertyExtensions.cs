// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        {
            if (property.DeclaringEntityType.BaseType != null
                || property.IsNullable)
            {
                return true;
            }

            return property.IsPrimaryKey()
                ? false
                : IsOwnedByDerivedType(property.DeclaringEntityType);
        }

        private static bool IsOwnedByDerivedType(IEntityType entityType)
        {
            var ownerEntityType = entityType.FindPrimaryKey()?.Properties.First()
                ?.FindSharedTableLink()?.PrincipalEntityType;

            if (ownerEntityType?.BaseType != null)
            {
                return true;
            }

            return ownerEntityType != null && IsOwnedByDerivedType(ownerEntityType);
        }
    }
}
