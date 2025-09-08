// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the collation for an entity based on the applied <see cref="XGCollationAttribute" />.
    /// </summary>
    public class TableCollationAttributeConvention : TypeAttributeConventionBase<XGCollationAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="TableCollationAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public TableCollationAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        protected override void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            XGCollationAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => entityTypeBuilder.UseCollation(attribute.CollationName, attribute.DelegationModes);
    }
}
