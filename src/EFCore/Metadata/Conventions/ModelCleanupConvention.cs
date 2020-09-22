// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that removes any state that is only used during model building.
    /// </summary>
    public class ModelCleanupConvention : IModelFinalizingConvention
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

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            RemoveEntityTypesUnreachableByNavigations(modelBuilder, context);
            RemoveNavigationlessForeignKeys(modelBuilder);
            RemoveModelBuildingAnnotations(modelBuilder);
        }

        private void RemoveEntityTypesUnreachableByNavigations(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var model = modelBuilder.Metadata;
            var rootEntityTypes = GetRoots(model, ConfigurationSource.DataAnnotation);
            using (context.DelayConventions())
            {
                foreach (var orphan in new GraphAdapter(model).GetUnreachableVertices(rootEntityTypes))
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
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes()
                .Where(e => !((EntityType)e).IsImplicitlyCreatedJoinEntityType))
            {
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
                {
                    if (foreignKey.PrincipalToDependent == null
                        && foreignKey.DependentToPrincipal == null
                        && !foreignKey.GetReferencingSkipNavigations().Any())
                    {
                        entityType.Builder.HasNoRelationship(foreignKey, fromDataAnnotation: true);
                    }
                }
            }
        }

        private void RemoveModelBuildingAnnotations(IConventionModelBuilder modelBuilder)
        {
            modelBuilder.Metadata.RemoveAnnotation(CoreAnnotationNames.DerivedTypes);
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                entityType.RemoveAnnotation(CoreAnnotationNames.AmbiguousNavigations);
                entityType.RemoveAnnotation(CoreAnnotationNames.NavigationCandidates);
            }
        }

        private sealed class GraphAdapter : Graph<IConventionEntityType>
        {
            private readonly IConventionModel _model;

            public GraphAdapter([NotNull] IConventionModel model)
            {
                _model = model;
            }

            public override IEnumerable<IConventionEntityType> Vertices
                => _model.GetEntityTypes();

            public override IEnumerable<IConventionEntityType> GetOutgoingNeighbors(IConventionEntityType from)
                => from.GetForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.PrincipalEntityType)
                    .Union(
                        from.GetReferencingForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.DeclaringEntityType))
                    .Union(from.GetSkipNavigations().Where(sn => sn.ForeignKey != null).Select(sn => sn.ForeignKey.DeclaringEntityType))
                    .Union(from.GetSkipNavigations().Where(sn => sn.TargetEntityType != null).Select(sn => sn.TargetEntityType));

            public override IEnumerable<IConventionEntityType> GetIncomingNeighbors(IConventionEntityType to)
                => to.GetForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.PrincipalEntityType)
                    .Union(to.GetReferencingForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.DeclaringEntityType))
                    .Union(to.GetSkipNavigations().Where(sn => sn.ForeignKey != null).Select(sn => sn.ForeignKey.DeclaringEntityType))
                    .Union(to.GetSkipNavigations().Where(sn => sn.TargetEntityType != null).Select(sn => sn.TargetEntityType));

            public override void Clear()
            {
            }
        }
    }
}
