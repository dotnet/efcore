// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqliteCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        public SqliteCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            var sqliteTranslators = new List<IMethodCallTranslator>
            {
                new SqliteMathAbsTranslator(),
                new SqliteStringToLowerTranslator(),
                new SqliteStringToUpperTranslator()
            };

            AddTranslators(sqliteTranslators);
        }
    }
}
