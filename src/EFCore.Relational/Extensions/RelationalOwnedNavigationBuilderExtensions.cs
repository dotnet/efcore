// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
            this OwnedNavigationBuilder ownedNavigationBuilder,
            string name,
            string? sql)
        {
            Check.NotNull(ownedNavigationBuilder, nameof(ownedNavigationBuilder));

            InternalCheckConstraintBuilder.HasCheckConstraint(
                  (IConventionEntityType)ownedNavigationBuilder.OwnedEntityType,
                  name,
                  sql,
                  ConfigurationSource.Explicit);

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
            this OwnedNavigationBuilder<TEntity, TDependentEntity> ownedNavigationBuilder,
            string name,
            string? sql)
            where TEntity : class
            where TDependentEntity : class
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)
                HasCheckConstraint((OwnedNavigationBuilder)ownedNavigationBuilder, name, sql);

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="ownedNavigationBuilder"> The navigation builder for the owned type. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="buildAction"> An action that performs configuration of the check constraint. </param>
        /// <returns> A builder to further configure the navigation. </returns>
        public static OwnedNavigationBuilder HasCheckConstraint(
            this OwnedNavigationBuilder ownedNavigationBuilder,
            string name,
            string sql,
            Action<CheckConstraintBuilder> buildAction)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(buildAction, nameof(buildAction));

            ownedNavigationBuilder.HasCheckConstraint(name, sql);

            buildAction(new CheckConstraintBuilder(ownedNavigationBuilder.OwnedEntityType.FindCheckConstraint(name)!));

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
        /// <param name="buildAction"> An action that performs configuration of the check constraint. </param>
        /// <returns> A builder to further configure the navigation. </returns>
        public static OwnedNavigationBuilder<TEntity, TDependentEntity> HasCheckConstraint<TEntity, TDependentEntity>(
            this OwnedNavigationBuilder<TEntity, TDependentEntity> ownedNavigationBuilder,
            string name,
            string sql,
            Action<CheckConstraintBuilder> buildAction)
            where TEntity : class
            where TDependentEntity : class
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)
                HasCheckConstraint((OwnedNavigationBuilder)ownedNavigationBuilder, name, sql, buildAction);
    }
}
