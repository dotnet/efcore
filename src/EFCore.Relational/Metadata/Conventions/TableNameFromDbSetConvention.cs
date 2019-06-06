// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///      A convention that configures the table name based on the <see cref="DbSet{TEntity}"/> property name.
    /// </summary>
    public class TableNameFromDbSetConvention : IEntityTypeAddedConvention, IEntityTypeBaseTypeChangedConvention
    {
        private readonly IDictionary<Type, DbSetProperty> _sets;

        /// <summary>
        ///     Creates a new instance of <see cref="TableNameFromDbSetConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public TableNameFromDbSetConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            _sets = dependencies.SetFinder.CreateClrTypeDbSetMapping(dependencies.ContextType);

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (_sets != null)
            {
                var entityType = entityTypeBuilder.Metadata;

                if (oldBaseType == null
                    && newBaseType != null)
                {
                    entityTypeBuilder.ToTable(null);
                }
                else if (oldBaseType != null
                         && newBaseType == null
                         && entityType.ClrType != null
                         && _sets.ContainsKey(entityType.ClrType))
                {
                    entityTypeBuilder.ToTable(_sets[entityType.ClrType].Name);
                }
            }
        }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.BaseType == null
                && entityType.ClrType != null
                && _sets.ContainsKey(entityType.ClrType))
            {
                entityTypeBuilder.ToTable(_sets[entityType.ClrType].Name);
            }
        }
    }
}
