// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteMemberTranslatorProvider : RelationalMemberTranslatorProvider
    {
        public SqliteMemberTranslatorProvider([NotNull] RelationalMemberTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;

            AddTranslators(
                new IMemberTranslator[]
                {
                    new SqliteDateTimeMemberTranslator(sqlExpressionFactory), new SqliteStringLengthTranslator(sqlExpressionFactory)
                });
        }
    }
}
