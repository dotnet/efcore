// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="OwnedNavigationBuilder" />.
    /// </summary>
    public static class RelationalOwnedNavigationBuilderExtensions
    {
        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="ownedNavigationBuilder"> The navigation builder for the owned type. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the navigation. </returns>
        public static OwnedNavigationBuilder HasCheckConstraint(
            [NotNull] this OwnedNavigationBuilder ownedNavigationBuilder,
            [NotNull] string name,
            [CanBeNull] string sql)
        {
            Check.NotNull(ownedNavigationBuilder, nameof(ownedNavigationBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var entityType = ownedNavigationBuilder.OwnedEntityType;

            var constraint = entityType.FindCheckConstraint(name);
            if (constraint != null)
            {
                if (constraint.Sql == sql)
                {
                    ((CheckConstraint)constraint).UpdateConfigurationSource(ConfigurationSource.Explicit);
                    return ownedNavigationBuilder;
                }

                entityType.RemoveCheckConstraint(name);
            }

            if (sql != null)
            {
                entityType.AddCheckConstraint(name, sql);
            }

            return ownedNavigationBuilder;
        }

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type owning the relationship. </typeparam>
        /// <typeparam name="TDependentEntity"> The dependent entity type of the relationship. </typeparam>
        /// <param name="ownedNavigationBuilder"> The navigation builder for the owned type. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the navigation. </returns>
        public static OwnedNavigationBuilder<TEntity, TDependentEntity> HasCheckConstraint<TEntity, TDependentEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TDependentEntity> ownedNavigationBuilder,
            [NotNull] string name,
            [CanBeNull] string sql)
            where TEntity : class
            where TDependentEntity : class
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)
                HasCheckConstraint((OwnedNavigationBuilder)ownedNavigationBuilder, name, sql);
    }
}
