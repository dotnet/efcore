// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Sql
{
    public interface ISqlGenerator
    {
        /// <summary>
        ///     Generates a SQL query for the given parameter values.
        /// </summary>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <returns>
        ///     The SQL query.
        /// </returns>
        CosmosSqlQuery GenerateSqlQuery([NotNull] IReadOnlyDictionary<string, object> parameterValues);
    }
}
