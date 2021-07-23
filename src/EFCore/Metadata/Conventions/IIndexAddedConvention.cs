// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            IConventionIndexBuilder indexBuilder,
            IConventionContext<IConventionIndexBuilder> context);
    }
}
