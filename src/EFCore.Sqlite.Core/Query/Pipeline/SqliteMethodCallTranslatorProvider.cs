// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqliteMethodCallTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IValueConverterSelector valueConverterSelector,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new SqliteMathTranslator(sqlExpressionFactory, valueConverterSelector),
                    new SqliteDateTimeAddTranslator(sqlExpressionFactory),
                    new SqliteStringMethodTranslator(sqlExpressionFactory, valueConverterSelector),
                });
        }
    }
}
