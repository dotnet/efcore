// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Commands;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface ISqlQueryGenerator
    {
        RelationalCommand GenerateSql([NotNull] IDictionary<string, object> parameterValues);

        IRelationalValueBufferFactory CreateValueBufferFactory(
            [NotNull] IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory,
            [NotNull] DbDataReader dataReader);
    }
}
