// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface IQuerySqlGenerator
    {
        IRelationalCommand GenerateSql([NotNull] IReadOnlyDictionary<string, object> parameterValues);

        IRelationalValueBufferFactory CreateValueBufferFactory(
            [NotNull] IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory,
            [NotNull] DbDataReader dataReader);
    }
}
