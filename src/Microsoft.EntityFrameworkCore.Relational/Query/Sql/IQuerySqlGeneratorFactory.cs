// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    public interface IQuerySqlGeneratorFactory
    {
        IQuerySqlGenerator CreateDefault([NotNull] SelectExpression selectExpression);

        IQuerySqlGenerator CreateFromSql(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] Expression arguments);
    }
}
