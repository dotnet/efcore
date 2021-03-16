// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures a property as having a backing field
    ///     based on the <see cref="BackingFieldAttribute" /> attribute.
    /// </summary>
    public class BackingFieldAttributeConvention : PropertyAttributeConventionBase<BackingFieldAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="BackingFieldAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public BackingFieldAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
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
            BackingFieldAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            propertyBuilder.HasField(attribute.Name, fromDataAnnotation: true);
        }
    }
}
