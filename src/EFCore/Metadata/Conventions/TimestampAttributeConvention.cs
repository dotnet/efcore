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
    ///     A convention that configures the property as a concurrency token if a <see cref="TimestampAttribute" /> is applied to it.
    /// </summary>
    public class TimestampAttributeConvention : PropertyAttributeConventionBase<TimestampAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="TimestampAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public TimestampAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            TimestampAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, fromDataAnnotation: true);
            propertyBuilder.IsConcurrencyToken(true, fromDataAnnotation: true);
        }
    }
}
