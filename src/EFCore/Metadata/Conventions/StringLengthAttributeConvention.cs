// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the maximum length based on the <see cref="StringLengthAttribute" /> applied on the property.
    /// </summary>
    public class StringLengthAttributeConvention : PropertyAttributeConventionBase<StringLengthAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="StringLengthAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public StringLengthAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
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
            StringLengthAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            if (attribute.MaximumLength > 0)
            {
                propertyBuilder.HasMaxLength(attribute.MaximumLength, fromDataAnnotation: true);
            }
        }
    }
}
