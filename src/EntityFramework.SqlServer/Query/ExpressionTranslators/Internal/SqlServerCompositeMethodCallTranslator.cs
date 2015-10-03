// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqlServerCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
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
            new SqlServerConvertTranslator()
        };

        // ReSharper disable once SuggestBaseTypeForParameter
        public SqlServerCompositeMethodCallTranslator([NotNull] ILogger<SqlServerCompositeMethodCallTranslator> logger)
            : base(logger)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
