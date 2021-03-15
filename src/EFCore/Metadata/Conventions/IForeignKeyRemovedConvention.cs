// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a foreign key is removed.
    /// </summary>
    public interface IForeignKeyRemovedConvention : IConvention
    {
        /// <summary>
        ///     Called after a foreign key is removed.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="foreignKey"> The removed foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionForeignKey foreignKey,
            [NotNull] IConventionContext<IConventionForeignKey> context);
    }
}
