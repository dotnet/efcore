// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the character set for an entity based on the applied <see cref="XGCharSetAttribute" />.
    /// </summary>
    public class TableCharSetAttributeConvention : TypeAttributeConventionBase<XGCharSetAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="TableCharSetAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public TableCharSetAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        protected override void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            XGCharSetAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => entityTypeBuilder.HasCharSet(attribute.CharSetName, attribute.DelegationModes);
    }
}
