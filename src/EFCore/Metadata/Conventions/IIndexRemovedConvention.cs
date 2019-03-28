// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an index is removed from the entity type.
    /// </summary>
    public interface IIndexRemovedConvention : IConvention
    {
        /// <summary>
        ///     Called after an index is removed.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="index"> The removed index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessIndexRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionIndex index,
            [NotNull] IConventionContext<IConventionIndex> context);
    }
}
