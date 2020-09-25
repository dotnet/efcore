// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the maximum length based on the <see cref="MaxLengthAttribute" /> applied on the property.
    /// </summary>
    public class MaxLengthAttributeConvention : PropertyAttributeConventionBase<MaxLengthAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="MaxLengthAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public MaxLengthAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            MaxLengthAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            if (attribute.Length > 0)
            {
                propertyBuilder.HasMaxLength(attribute.Length, fromDataAnnotation: true);
            }
        }
    }
}
