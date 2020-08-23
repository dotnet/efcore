// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a model is being finalized.
    /// </summary>
    public interface IModelFinalizingConvention : IConvention
    {
        /// <summary>
        ///     Called when a model is being finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessModelFinalizing(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] IConventionContext<IConventionModelBuilder> context);
    }
}
