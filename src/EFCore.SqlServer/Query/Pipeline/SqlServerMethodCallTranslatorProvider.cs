// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqlServerMethodCallTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IValueConverterSelector valueConverterSelector,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            AddTranslators(new IMethodCallTranslator[]
            {
                new SqlServerMathTranslator(sqlExpressionFactory, valueConverterSelector),
                new SqlServerNewGuidTranslator(sqlExpressionFactory),
                new SqlServerStringMethodTranslator(sqlExpressionFactory, valueConverterSelector),
                new SqlServerDateTimeMethodTranslator(sqlExpressionFactory),
                new SqlServerDateDiffFunctionsTranslator(sqlExpressionFactory, valueConverterSelector),
                new SqlServerConvertTranslator(sqlExpressionFactory),
                new SqlServerObjectToStringTranslator(sqlExpressionFactory),
                new SqlServerFullTextSearchFunctionsTranslator(sqlExpressionFactory),
            });
        }
    }
}
