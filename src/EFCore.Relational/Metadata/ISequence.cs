// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the model.
    /// </summary>
    public interface ISequence : IReadOnlySequence, IAnnotatable
    {
        /// <summary>
        ///     Gets the model in which this sequence is defined.
        /// </summary>
        new IModel Model { get; }
    }
}
