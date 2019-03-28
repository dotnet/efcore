// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an index is added to the entity type.
    /// </summary>
    public interface IIndexAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after an index is added to the entity type.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessIndexAdded(
            [NotNull] IConventionIndexBuilder indexBuilder,
            [NotNull] IConventionContext<IConventionIndexBuilder> context);
    }
}
