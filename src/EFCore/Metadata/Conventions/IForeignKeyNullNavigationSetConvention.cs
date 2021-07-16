// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a navigation is set to <see langword="null"/> on a foreign key.
    /// </summary>
    public interface IForeignKeyNullNavigationSetConvention : IConvention
    {
        /// <summary>
        ///     Called after a navigation is set to <see langword="null"/> on a foreign key.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the <see langword="null"/> navigation would be pointing to the principal entity type.
        /// </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyNullNavigationSet(
            IConventionForeignKeyBuilder relationshipBuilder,
            bool pointsToPrincipal,
            IConventionContext<IConventionNavigation> context);
    }
}
