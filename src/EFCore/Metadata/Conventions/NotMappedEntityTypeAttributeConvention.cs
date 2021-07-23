// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that ignores entity types that have the <see cref="NotMappedAttribute" />.
    /// </summary>
    public class NotMappedEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<NotMappedAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NotMappedEntityTypeAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NotMappedEntityTypeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
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
            NotMappedAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            if (entityTypeBuilder.ModelBuilder.Ignore(entityTypeBuilder.Metadata.Name, fromDataAnnotation: true) != null)
            {
                context.StopProcessing();
            }
        }
    }
}
