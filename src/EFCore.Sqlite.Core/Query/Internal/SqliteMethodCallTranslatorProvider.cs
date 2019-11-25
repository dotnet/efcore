// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqliteMethodCallTranslatorProvider([NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;

            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new SqliteMathTranslator(sqlExpressionFactory),
                    new SqliteDateTimeAddTranslator(sqlExpressionFactory),
                    new SqliteStringMethodTranslator(sqlExpressionFactory)
                });
        }
    }
}
