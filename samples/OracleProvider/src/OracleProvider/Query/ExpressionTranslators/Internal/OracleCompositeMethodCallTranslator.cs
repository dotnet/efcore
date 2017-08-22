// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class OracleCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new OracleContainsOptimizedTranslator(),
            new OracleConvertTranslator(),
            new OracleDateAddTranslator(),
            new OracleEndsWithOptimizedTranslator(),
            new OracleMathTranslator(),
            new OracleNewGuidTranslator(),
            new OracleObjectToStringTranslator(),
            new OracleStartsWithOptimizedTranslator(),
            new OracleStringIsNullOrWhiteSpaceTranslator(),
            new OracleStringReplaceTranslator(),
            new OracleStringSubstringTranslator(),
            new OracleStringToLowerTranslator(),
            new OracleStringToUpperTranslator(),
            new OracleStringTrimEndTranslator(),
            new OracleStringTrimStartTranslator(),
            new OracleStringTrimTranslator()
        };

        public OracleCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
