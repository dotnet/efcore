// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new SqlServerContainsOptimizedTranslator(),
            new SqlServerConvertTranslator(),
            new SqlServerDateAddTranslator(),
            new SqlServerDateDiffTranslator(),
            new SqlServerEndsWithOptimizedTranslator(),
            new SqlServerFullTextSearchMethodCallTranslator(),
            new SqlServerMathTranslator(),
            new SqlServerNewGuidTranslator(),
            new SqlServerObjectToStringTranslator(),
            new SqlServerStartsWithOptimizedTranslator(),
            new SqlServerStringIsNullOrWhiteSpaceTranslator(),
            new SqlServerStringReplaceTranslator(),
            new SqlServerStringSubstringTranslator(),
            new SqlServerStringToLowerTranslator(),
            new SqlServerStringToUpperTranslator(),
            new SqlServerStringTrimEndTranslator(),
            new SqlServerStringTrimStartTranslator(),
            new SqlServerStringTrimTranslator(),
            new SqlServerStringIndexOfTranslator(),
            new SqlServerStringConcatMethodCallTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
