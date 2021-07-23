// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the column comment for a property or field based on the applied <see cref="CommentAttribute" />.
    /// </summary>
    public class RelationalColumnCommentAttributeConvention : PropertyAttributeConventionBase<CommentAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalColumnCommentAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalColumnCommentAttributeConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
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
            CommentAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Comment))
            {
                propertyBuilder.HasComment(attribute.Comment, fromDataAnnotation: true);
            }
        }
    }
}
