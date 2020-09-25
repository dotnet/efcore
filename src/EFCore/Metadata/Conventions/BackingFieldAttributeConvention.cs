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


        /// <inheritdoc />
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
