// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IRelationalCommandBuilder : IInfrastructure<IndentedStringBuilder>
    {
        void AddParameter([NotNull] IRelationalParameter relationalParameter);

        IRelationalParameter CreateParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Func<IRelationalTypeMapper, RelationalTypeMapping> mapType,
            bool? nullable,
            [CanBeNull] string invariantName);

        IRelationalCommand Build();
    }
}
