// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface IQuerySqlGeneratorFactory
    {
        IQuerySqlGenerator CreateDefault([NotNull] SelectExpression selectExpression);

        IQuerySqlGenerator CreateFromSql(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] string argumentsParameterName);
    }
}
