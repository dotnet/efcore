// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a skip navigation foreign key is changed.
    /// </summary>
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
            [NotNull] IConventionSkipNavigationBuilder skipNavigationBuilder,
            [CanBeNull] IConventionForeignKey foreignKey,
            [CanBeNull] IConventionForeignKey oldForeignKey,
            [NotNull] IConventionContext<IConventionForeignKey> context);
    }
}
