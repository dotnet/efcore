// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
            [NotNull] IConventionIndexBuilder indexBuilder,
            [NotNull] IConventionContext<bool?> context);
    }
}
