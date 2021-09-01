// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a skip navigation foreign key is changed.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    public interface ISkipNavigationForeignKeyChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after a skip navigation inverse is changed.
        /// </summary>
        /// <param name="skipNavigationBuilder"> The builder for the skip navigation. </param>
        /// <param name="foreignKey"> The current skip navigation foreign key. </param>
        /// <param name="oldForeignKey"> The old skip navigation foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey,
            IConventionContext<IConventionForeignKey> context);
    }
}
