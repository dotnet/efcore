// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the Precision based on the <see cref="PrecisionAttribute" /> applied on the property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    public class PrecisionAttributeConvention : PropertyAttributeConventionBase<PrecisionAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="PrecisionAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public PrecisionAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
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
            PrecisionAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            propertyBuilder.HasPrecision(attribute.Precision, fromDataAnnotation: true);

            if (attribute.Scale.HasValue)
            {
                propertyBuilder.HasScale(attribute.Scale, fromDataAnnotation: true);
            }
        }
    }
}
