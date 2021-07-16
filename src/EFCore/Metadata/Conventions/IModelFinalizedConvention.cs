// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        IModel ProcessModelFinalized(IModel model);
    }
}
