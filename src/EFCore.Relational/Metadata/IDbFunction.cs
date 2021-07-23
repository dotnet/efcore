// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in a model.
    /// </summary>
    public interface IDbFunction : IReadOnlyDbFunction, IAnnotatable
    {
        /// <summary>
        ///     Gets the model in which this function is defined.
        /// </summary>
        new IModel Model { get; }

        /// <summary>
        ///     Gets the parameters for this function
        /// </summary>
        new IReadOnlyList<IDbFunctionParameter> Parameters { get; }

        /// <summary>
        ///     Gets the associated <see cref="IStoreFunction" />.
        /// </summary>
        IStoreFunction StoreFunction { get; }
    }
}
