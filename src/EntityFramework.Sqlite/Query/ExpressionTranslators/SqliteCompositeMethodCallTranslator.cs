// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.Query.ExpressionTranslators
{
    public class SqliteCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        public SqliteCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            var sqliteTranslators = new List<IMethodCallTranslator>
            {
                new MathAbsTranslator(),
                new StringToLowerTranslator(),
                new StringToUpperTranslator(),
            };

            AddTranslators(sqliteTranslators);
        }
    }
}
