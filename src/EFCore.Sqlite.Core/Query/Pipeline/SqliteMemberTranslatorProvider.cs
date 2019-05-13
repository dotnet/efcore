// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteMemberTranslatorProvider : RelationalMemberTranslatorProvider
    {
        public SqliteMemberTranslatorProvider(
            ISqlExpressionFactory sqlExpressionFactory,
            IEnumerable<IMemberTranslatorPlugin> plugins)
            : base(sqlExpressionFactory, plugins)
        {
            AddTranslators(
                new IMemberTranslator[]
                {
                    new SqliteDateTimeMemberTranslator(sqlExpressionFactory),
                    new SqliteStringLengthTranslator(sqlExpressionFactory)
                });
        }
    }
}
