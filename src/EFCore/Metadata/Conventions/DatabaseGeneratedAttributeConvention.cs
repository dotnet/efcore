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
    ///     A convention that configures a property as <see cref="ValueGenerated.OnAdd" /> if
    ///     <see cref="DatabaseGeneratedOption.Identity" /> is specified, <see cref="ValueGenerated.OnAddOrUpdate" /> if
    ///     <see cref="DatabaseGeneratedOption.Computed" /> is specified or <see cref="ValueGenerated.Never" /> if
    ///     <see cref="DatabaseGeneratedOption.None" /> is specified using a <see cref="DatabaseGeneratedAttribute" />.
    /// </summary>
    public class DatabaseGeneratedAttributeConvention : PropertyAttributeConventionBase<DatabaseGeneratedAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DatabaseGeneratedAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public DatabaseGeneratedAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
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
            DatabaseGeneratedAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            var valueGenerated =
                attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity
                    ? ValueGenerated.OnAdd
                    : attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed
                        ? ValueGenerated.OnAddOrUpdate
                        : ValueGenerated.Never;

            propertyBuilder.ValueGenerated(valueGenerated, fromDataAnnotation: true);
        }
    }
}
