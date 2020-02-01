// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionForeignKey" />.
    /// </summary>
    public static class ConventionForeignKeyExtensions
    {
        /// <summary>
        ///     Gets the entity type related to the given one.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="entityType"> One of the entity types related by the foreign key. </param>
        /// <returns> The entity type related to the given one. </returns>
        public static IConventionEntityType GetRelatedEntityType(
            [NotNull] this IConventionForeignKey foreignKey, [NotNull] IConventionEntityType entityType)
            => (IConventionEntityType)((IForeignKey)foreignKey).GetRelatedEntityType(entityType);

        /// <summary>
        ///     Returns a navigation associated with this foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <returns>
        ///     A navigation associated with this foreign key or <c>null</c>.
        /// </returns>
        public static IConventionNavigation GetNavigation([NotNull] this IConventionForeignKey foreignKey, bool pointsToPrincipal)
            => pointsToPrincipal ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent;
    }
}
