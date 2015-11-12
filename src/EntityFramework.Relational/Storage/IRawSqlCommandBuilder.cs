// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRawSqlCommandBuilder
    {
        IRelationalCommand Build(
            [NotNull] string sql,
            [CanBeNull] IReadOnlyList<object> parameters = null);
    }
}
