// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqlServerMethodCallTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            AddTranslators(new IMethodCallTranslator[]
            {
                new SqlServerConvertTranslator(sqlExpressionFactory),
                new SqlServerDateTimeMethodTranslator(sqlExpressionFactory),
                new SqlServerDateDiffFunctionsTranslator(sqlExpressionFactory),
                new SqlServerFullTextSearchFunctionsTranslator(sqlExpressionFactory),
                new SqlServerIsDateFunctionTranslator(sqlExpressionFactory),
                new SqlServerMathTranslator(sqlExpressionFactory),
                new SqlServerNewGuidTranslator(sqlExpressionFactory),
                new SqlServerObjectToStringTranslator(sqlExpressionFactory),
                new SqlServerStringMethodTranslator(sqlExpressionFactory),
            });
        }
    }
}
