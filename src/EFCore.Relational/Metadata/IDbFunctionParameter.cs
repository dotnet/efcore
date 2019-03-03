// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function parameter in an <see cref="IDbFunction" />.
    /// </summary>
    public interface IDbFunctionParameter
    {
        IDbFunction Parent { get; }

        string Name { get; }

        Type ClrType { get; }

        bool SupportsNullPropagation { get; }

        string StoreType { get; }

        RelationalTypeMapping TypeMapping { get; }
    }
}
