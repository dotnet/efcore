// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures column name and type for a property based on the applied <see cref="ColumnAttribute" />.
    /// </summary>
    public class RelationalColumnAttributeConvention : PropertyAttributeConventionBase<ColumnAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalColumnAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalColumnAttributeConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="clrMember"> The member that has the attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            ColumnAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                propertyBuilder.HasColumnName(attribute.Name, fromDataAnnotation: true);
            }

            if (!string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                propertyBuilder.HasColumnType(attribute.TypeName, fromDataAnnotation: true);
            }
        }
    }
}
