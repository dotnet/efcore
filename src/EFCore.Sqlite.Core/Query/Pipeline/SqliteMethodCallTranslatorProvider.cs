// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqliteMethodCallTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new SqliteMathTranslator(sqlExpressionFactory),
                    new SqliteDateTimeAddTranslator(sqlExpressionFactory),
                    new SqliteStringMethodTranslator(sqlExpressionFactory),
                });
        }
    }
}
