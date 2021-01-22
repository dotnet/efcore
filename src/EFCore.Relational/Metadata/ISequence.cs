// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a database sequence in the model.
    /// </summary>
    public interface ISequence : IReadOnlySequence, IAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IModel" /> in which this sequence is defined.
        /// </summary>
        new IModel Model { get; }
    }
}
