// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class RelationalConventionSetBuilder : IConventionSetBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalConnection" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalConventionSetBuilder([NotNull] RelationalConventionSetBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet AddConventions(ConventionSet conventionSet)
        {
            var typeMapper = Dependencies.TypeMapper;

            RelationshipDiscoveryConvention relationshipDiscoveryConvention
                = new RelationalRelationshipDiscoveryConvention(typeMapper);

            InversePropertyAttributeConvention inversePropertyAttributeConvention
                = new RelationalInversePropertyAttributeConvention(typeMapper);

            ReplaceConvention(
                conventionSet.EntityTypeAddedConventions,
                (PropertyDiscoveryConvention)new RelationalPropertyDiscoveryConvention(typeMapper));
            ReplaceConvention(conventionSet.EntityTypeAddedConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.EntityTypeAddedConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.EntityTypeIgnoredConventions, inversePropertyAttributeConvention);

            ValueGeneratorConvention valueGeneratorConvention = new RelationalValueGeneratorConvention(Dependencies.AnnotationProvider);
            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, relationshipDiscoveryConvention);
            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.EntityTypeMemberIgnoredConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.EntityTypeMemberIgnoredConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.PrimaryKeySetConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions,
                (ForeignKeyAttributeConvention)new RelationalForeignKeyAttributeConvention(typeMapper));
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.NavigationAddedConventions, inversePropertyAttributeConvention);
            ReplaceConvention(conventionSet.NavigationAddedConventions, relationshipDiscoveryConvention);

            ReplaceConvention(conventionSet.NavigationRemovedConventions, relationshipDiscoveryConvention);

            ReplaceConvention(
                conventionSet.ModelBuiltConventions,
                (PropertyMappingValidationConvention)new RelationalPropertyMappingValidationConvention(typeMapper));

            var relationalColumnAttributeConvention = new RelationalColumnAttributeConvention();
            conventionSet.PropertyAddedConventions.Add(relationalColumnAttributeConvention);

            var sharedTableConvention = new SharedTableConvention(Dependencies.AnnotationProvider);
            conventionSet.EntityTypeAddedConventions.Add(new RelationalTableAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(sharedTableConvention);

            conventionSet.BaseEntityTypeSetConventions.Add(new DiscriminatorConvention());

            conventionSet.BaseEntityTypeSetConventions.Add(
                new TableNameFromDbSetConvention(Dependencies.Context?.Context, Dependencies.SetFinder));

            conventionSet.EntityTypeAnnotationSetConventions.Add(sharedTableConvention);

            conventionSet.PropertyFieldChangedConventions.Add(relationalColumnAttributeConvention);

            conventionSet.PropertyAnnotationSetConventions.Add((RelationalValueGeneratorConvention)valueGeneratorConvention);

            conventionSet.ForeignKeyUniquenessConventions.Add(sharedTableConvention);
            conventionSet.ForeignKeyOwnershipConventions.Add(sharedTableConvention);

            return conventionSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ReplaceConvention<T1, T2>([NotNull] IList<T1> conventionsList, [NotNull] T2 newConvention)
            where T2 : T1
        {
            var oldConvention = conventionsList.OfType<T2>().FirstOrDefault();
            if (oldConvention == null)
            {
                throw new InvalidOperationException();
            }
            var index = conventionsList.IndexOf(oldConvention);
            conventionsList.RemoveAt(index);
            conventionsList.Insert(index, newConvention);
        }
    }
}
