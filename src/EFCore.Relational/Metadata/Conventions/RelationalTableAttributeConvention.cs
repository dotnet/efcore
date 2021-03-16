// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the table name and schema for entity types based on the applied <see cref="TableAttribute" />.
    /// </summary>
    public class RelationalTableAttributeConvention : EntityTypeAttributeConventionBase<TableAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTableAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalTableAttributeConvention(
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
            TableAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Schema))
            {
                entityTypeBuilder.ToTable(attribute.Name, attribute.Schema, fromDataAnnotation: true);
            }
            else if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                entityTypeBuilder.ToTable(attribute.Name, fromDataAnnotation: true);
            }
        }
    }
}
