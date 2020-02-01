// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerMemberTranslatorProvider : RelationalMemberTranslatorProvider
    {
        public SqlServerMemberTranslatorProvider([NotNull] RelationalMemberTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;

            AddTranslators(
                new IMemberTranslator[]
                {
                    new SqlServerDateTimeMemberTranslator(sqlExpressionFactory),
                    new SqlServerStringMemberTranslator(sqlExpressionFactory)
                });
        }
    }
}
