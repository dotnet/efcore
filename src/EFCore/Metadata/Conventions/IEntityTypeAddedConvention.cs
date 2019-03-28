// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an entity type is added to the model.
    /// </summary>
    public interface IEntityTypeAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessEntityTypeAdded(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionContext<IConventionEntityTypeBuilder> context);
    }
}
