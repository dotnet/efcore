// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqlServerCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        public SqlServerCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            var sqlServerTranslators = new List<IMethodCallTranslator>
            {
                new SqlServerNewGuidTranslator(),
                new SqlServerStringSubstringTranslator(),
                new SqlServerMathAbsTranslator(),
                new SqlServerMathCeilingTranslator(),
                new SqlServerMathFloorTranslator(),
                new SqlServerMathPowerTranslator(),
                new SqlServerMathRoundTranslator(),
                new SqlServerMathTruncateTranslator(),
                new SqlServerStringReplaceTranslator(),
                new SqlServerStringToLowerTranslator(),
                new SqlServerStringToUpperTranslator(),
                new SqlServerConvertTranslator(),
            };

            AddTranslators(sqlServerTranslators);
        }
    }
}
