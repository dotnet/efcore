// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the comment of table for entity types based on the applied <see cref="DescriptionAttribute" />.
    /// </summary>
    public class RelationalTableDescriptionAttributeConvention : EntityTypeAttributeConventionBase<DescriptionAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTableDescriptionAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalTableDescriptionAttributeConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
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
            DescriptionAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Description))
            {
                entityTypeBuilder.HasComment(attribute.Description);
            }
        }
    }
}
