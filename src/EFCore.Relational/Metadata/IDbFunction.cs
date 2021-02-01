// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in an <see cref="IModel" /> in
    ///     the a form that can be mutated while the model is being built.
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
