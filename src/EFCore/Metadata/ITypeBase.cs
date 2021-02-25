// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

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
