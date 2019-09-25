// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an entity type is removed from the model.
    /// </summary>
    public interface IEntityTypeRemovedConvention : IConvention
    {
        /// <summary>
        ///     Called after an entity type is removed from the model.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="entityType"> The removed entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessEntityTypeRemoved(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] IConventionEntityType entityType,
            [NotNull] IConventionContext<IConventionEntityType> context);
    }
}
