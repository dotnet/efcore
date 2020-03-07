// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed after a model is finalized and can no longer be mutated.
    /// </summary>
    public interface IModelFinalizedConvention : IConvention
    {
        /// <summary>
        ///     Called after a model is finalized and can no longer be mutated.
        /// </summary>
        /// <param name="model"> The model. </param>
        IModel ProcessModelFinalized([NotNull] IModel model);
    }
}
