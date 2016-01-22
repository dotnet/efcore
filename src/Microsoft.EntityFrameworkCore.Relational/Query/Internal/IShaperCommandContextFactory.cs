// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public interface IShaperCommandContextFactory
    {
        ShaperCommandContext Create([NotNull] Func<IQuerySqlGenerator> sqlGeneratorFunc);
    }
}
