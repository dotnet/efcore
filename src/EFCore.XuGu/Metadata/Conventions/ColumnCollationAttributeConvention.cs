// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the column's collation for a property or field based on the applied <see cref="XGCollationAttribute" />.
    /// </summary>
    public class ColumnCollationAttributeConvention : PropertyAttributeConventionBase<XGCollationAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ColumnCollationAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ColumnCollationAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            XGCollationAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
            => propertyBuilder.UseCollation(attribute.CollationName);
    }
}
