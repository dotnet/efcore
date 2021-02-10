// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a <see cref="IReadOnlyDbFunction" /> parameter.
    /// </summary>
    public interface IReadOnlyDbFunctionParameter : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the <see cref="IReadOnlyDbFunction" /> to which this parameter belongs.
        /// </summary>
        IReadOnlyDbFunction Function { get; }

        /// <summary>
        ///     Gets the parameter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the parameter type.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     Gets the store type of this parameter.
        /// </summary>
        string? StoreType { get; }

        /// <summary>
        ///     Gets the value which indicates whether parameter propagates nullability, meaning if it's value is null the database function itself
        ///     returns null.
        /// </summary>
        bool PropagatesNullability { get; }

        /// <summary>
        ///     Gets the <see cref="RelationalTypeMapping" /> for this parameter.
        /// </summary>
        RelationalTypeMapping? TypeMapping { get; }
    }
}
