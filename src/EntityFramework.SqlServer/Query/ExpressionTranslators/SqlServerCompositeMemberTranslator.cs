// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Query.ExpressionTranslators;

namespace Microsoft.Data.Entity.SqlServer.Query.ExpressionTranslators
{
    public class SqlServerCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        public SqlServerCompositeMemberTranslator()
        {
            var sqlServerTranslators = new List<IMemberTranslator>
            {
                new StringLengthTranslator(),
                new DateTimeNowTranslator()
            };

            AddTranslators(sqlServerTranslators);
        }
    }
}
