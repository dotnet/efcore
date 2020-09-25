// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that sets the delete behavior to <see cref="DeleteBehavior.Cascade" /> for required foreign keys
    ///     and <see cref="DeleteBehavior.ClientSetNull" /> for optional ones.
    /// </summary>
    public class CascadeDeleteConvention : IForeignKeyAddedConvention, IForeignKeyRequirednessChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="CascadeDeleteConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public CascadeDeleteConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyAdded(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context)
        {
            var newRelationshipBuilder = relationshipBuilder.OnDelete(GetTargetDeleteBehavior(relationshipBuilder.Metadata));
            if (newRelationshipBuilder != null)
            {
                context.StopProcessingIfChanged(newRelationshipBuilder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
        {
            var newRelationshipBuilder = relationshipBuilder.OnDelete(GetTargetDeleteBehavior(relationshipBuilder.Metadata));
            context.StopProcessingIfChanged(newRelationshipBuilder?.Metadata.IsRequired);
        }

        /// <summary>
        ///     Returns the delete behavior to set for the given foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        protected virtual DeleteBehavior GetTargetDeleteBehavior([NotNull] IConventionForeignKey foreignKey)
            => foreignKey.IsRequired ? DeleteBehavior.Cascade : DeleteBehavior.ClientSetNull;
    }
}
