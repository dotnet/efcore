// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the entity types that have the <see cref="OwnedAttribute"/> as owned.
    /// </summary>
    public class OwnedEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<OwnedAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="OwnedEntityTypeAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public OwnedEntityTypeAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after an entity type is added to the model if it has an attribute.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected override void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            OwnedAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            if (entityTypeBuilder.Metadata.HasClrType())
            {
                entityTypeBuilder.ModelBuilder.Owned(entityTypeBuilder.Metadata.ClrType, fromDataAnnotation: true);
            }
        }
    }
}
