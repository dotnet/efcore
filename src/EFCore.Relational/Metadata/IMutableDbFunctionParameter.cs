// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function parametere in an <see cref="IDbFunction" />.
    /// </summary>
    public interface IMutableDbFunctionParameter : IDbFunctionParameter
    {
        new IMutableDbFunction Parent { get; }

        new bool SupportsNullPropagation { get; set; }

        new string StoreType { get; [param: CanBeNull] set; }

        new RelationalTypeMapping TypeMapping { get; [param: CanBeNull] set; }
    }
}
