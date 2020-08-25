// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the table name based on the <see cref="DbSet{TEntity}" /> property name.
    /// </summary>
    public class TableNameFromDbSetConvention : IEntityTypeAddedConvention, IEntityTypeBaseTypeChangedConvention, IModelFinalizingConvention
    {
        private readonly IDictionary<Type, string> _sets;

        /// <summary>
        ///     Creates a new instance of <see cref="TableNameFromDbSetConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public TableNameFromDbSetConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            _sets = new Dictionary<Type, string>();
            List<Type> ambiguousTypes = null;
            foreach (var set in dependencies.SetFinder.FindSets(dependencies.ContextType))
            {
                if (!_sets.ContainsKey(set.Type))
                {
                    _sets.Add(set.Type, set.Name);
                }
                else
                {
                    if (ambiguousTypes == null)
                    {
                        ambiguousTypes = new List<Type>();
                    }

                    ambiguousTypes.Add(set.Type);
                }
            }

            if (ambiguousTypes != null)
            {
                foreach (var type in ambiguousTypes)
                {
                    _sets.Remove(type);
                }
            }

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
            var entityType = entityTypeBuilder.Metadata;

            if (oldBaseType == null
                && newBaseType != null)
            {
                entityTypeBuilder.HasNoAnnotation(RelationalAnnotationNames.TableName);
            }
            else if (oldBaseType != null
                && newBaseType == null
                && entityType.ClrType != null
                && !entityType.HasSharedClrType
                && _sets.TryGetValue(entityType.ClrType, out var setName))
            {
                entityTypeBuilder.ToTable(setName);
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
                && !entityType.HasSharedClrType
                && _sets.TryGetValue(entityType.ClrType, out var setName))
            {
                entityTypeBuilder.ToTable(setName);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.GetTableName() != null
                    && entityType.GetViewNameConfigurationSource() != null
                    && _sets.ContainsKey(entityType.ClrType))
                {
                    // Undo the convention change if the entity type is mapped to a view
                    entityType.Builder.HasNoAnnotation(RelationalAnnotationNames.TableName);
                }
            }
        }
    }
}
