// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the discriminator value for entity types as the entity type name.
    /// </summary>
    public class CosmosDiscriminatorConvention : DiscriminatorConvention, IEntityTypeAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="CosmosDiscriminatorConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public CosmosDiscriminatorConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
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
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(context, nameof(context));

            var entityType = entityTypeBuilder.Metadata;
            if (entityTypeBuilder.Metadata.BaseType == null
                && !Any(entityTypeBuilder.Metadata.GetDerivedTypes()))
            {
                entityTypeBuilder.HasDiscriminator(typeof(string))
                    .HasValue(entityType, entityType.ShortName());
            }
        }

        private static bool Any([NotNull] IEnumerable source)
        {
            foreach (var _ in source)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (entityTypeBuilder.Metadata.BaseType != newBaseType)
            {
                return;
            }

            IConventionDiscriminatorBuilder discriminator;
            var entityType = entityTypeBuilder.Metadata;
            if (newBaseType == null)
            {
                discriminator = entityTypeBuilder.HasDiscriminator(typeof(string));
            }
            else
            {
                discriminator = newBaseType.Builder?.HasDiscriminator(typeof(string));

                if (newBaseType.BaseType == null)
                {
                    discriminator?.HasValue(newBaseType, newBaseType.ShortName());
                }
            }

            if (discriminator != null)
            {
                discriminator.HasValue(entityTypeBuilder.Metadata, entityTypeBuilder.Metadata.ShortName());
                SetDefaultDiscriminatorValues(entityType.GetDerivedTypes(), discriminator);
            }
        }

        /// <summary>
        ///     Called after an entity type is removed from the model.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="entityType"> The removed entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType,
            IConventionContext<IConventionEntityType> context)
        {
        }
    }
}
