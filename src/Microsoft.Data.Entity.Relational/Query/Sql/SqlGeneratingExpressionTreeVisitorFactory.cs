// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class SqlGeneratingExpressionTreeVisitorFactory : ISqlGeneratingExpressionTreeVisitorFactory
    {
        public virtual SqlGeneratingExpressionTreeVisitor Create(StringBuilder sql, IParameterFactory parameterFactory)
        {
            Check.NotNull(sql, "sql");
            Check.NotNull(parameterFactory, "parameterFactory");

            return new SqlGeneratingExpressionTreeVisitor(sql, parameterFactory);
        }
    }
}
