// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when the uniqueness for an index is changed.
    /// </summary>
    public interface IIndexUniquenessChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after the uniqueness for an index is changed.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessIndexUniquenessChanged(
            IConventionIndexBuilder indexBuilder,
            IConventionContext<bool?> context);
    }
}
