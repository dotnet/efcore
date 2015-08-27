// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.SqlServer.Query.Methods;

namespace Microsoft.Data.Entity.Sqlite.Query.ExpressionTranslators
{
    public class SqliteCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        public SqliteCompositeMemberTranslator()
        {
            var sqliteTranslators = new List<IMemberTranslator>
            {
                new StringLengthTranslator()
            };

            AddTranslators(sqliteTranslators);
        }
    }
}
