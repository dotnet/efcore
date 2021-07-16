// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a type in the model.
    /// </summary>
    public interface ITypeBase : IReadOnlyTypeBase, IAnnotatable
    {
        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        new IModel Model { get; }
    }
}
