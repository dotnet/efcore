// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _sqliteTranslators =
        {
            new SqliteContainsOptimizedTranslator(),
            new SqliteDateTimeAddTranslator(),
            new SqliteEndsWithOptimizedTranslator(),
            new SqliteMathTranslator(),
            new SqliteStartsWithOptimizedTranslator(),
            new SqliteStringIsNullOrWhiteSpaceTranslator(),
            new SqliteStringToLowerTranslator(),
            new SqliteStringToUpperTranslator(),
            new SqliteStringTrimEndTranslator(),
            new SqliteStringTrimStartTranslator(),
            new SqliteStringTrimTranslator(),
            new SqliteStringIndexOfTranslator(),
            new SqliteStringReplaceTranslator(),
            new SqliteStringSubstringTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            AddTranslators(_sqliteTranslators);
        }
    }
}
