// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the primary key for an entity type is changed.
    /// </summary>
    public interface IEntityTypePrimaryKeyChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the primary key for an entity type is changed.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newPrimaryKey"> The new primary key. </param>
        /// <param name="previousPrimaryKey"> The old primary key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessEntityTypePrimaryKeyChanged(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] IConventionKey newPrimaryKey,
            [CanBeNull] IConventionKey previousPrimaryKey,
            [NotNull] IConventionContext<IConventionKey> context);
    }
}
