// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalCommandBuilder
    {
        IndentedStringBuilder CommandTextBuilder { get; }

        IRelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable);

        IRelationalCommand BuildRelationalCommand();
    }
}
