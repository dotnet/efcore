// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that removes any state that is only used during model building.
    /// </summary>
    public class ModelCleanupConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ModelCleanupConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ModelCleanupConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            RemoveEntityTypesUnreachableByNavigations(modelBuilder, context);
            RemoveNavigationlessForeignKeys(modelBuilder);
            RemoveModelBuildingAnnotations(modelBuilder);
        }

        private void RemoveEntityTypesUnreachableByNavigations(
            IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            var model = modelBuilder.Metadata;
            var rootEntityTypes = GetRoots(model, ConfigurationSource.DataAnnotation);
            using (context.DelayConventions())
            {
                foreach (var orphan in new ModelNavigationsGraphAdapter(model).GetUnreachableVertices(rootEntityTypes))
                {
                    modelBuilder.HasNoEntityType(orphan, fromDataAnnotation: true);
                }
            }
        }

        private IReadOnlyList<IConventionEntityType> GetRoots(IConventionModel model, ConfigurationSource configurationSource)
        {
            var roots = new List<IConventionEntityType>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entityType in model.GetEntityTypes())
            {
                var currentConfigurationSource = entityType.GetConfigurationSource();
                if (currentConfigurationSource.Overrides(configurationSource))
                {
                    roots.Add(entityType);
                }
            }

            return roots;
        }

        private void RemoveNavigationlessForeignKeys(IConventionModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
                {
                    if (foreignKey.PrincipalToDependent == null
                        && foreignKey.DependentToPrincipal == null)
                    {
                        entityType.Builder.HasNoRelationship(foreignKey, fromDataAnnotation: true);
                    }
                }
            }
        }

        private void RemoveModelBuildingAnnotations(IConventionModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                entityType.RemoveAnnotation(CoreAnnotationNames.AmbiguousNavigations);
                entityType.RemoveAnnotation(CoreAnnotationNames.NavigationCandidates);
            }
        }
    }
}
