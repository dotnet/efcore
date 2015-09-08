// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface ISqlQueryGenerator
    {
        string GenerateSql([NotNull] IDictionary<string, object> parameterValues);

        IReadOnlyList<CommandParameter> Parameters { get; }

        IRelationalTypeMapper TypeMapper { get; }

        IRelationalValueBufferFactory CreateValueBufferFactory(
            [NotNull] IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory,
            [NotNull] DbDataReader dataReader);
    }
}
