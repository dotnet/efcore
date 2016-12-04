// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
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
            new SqliteEndsWithOptimizedTranslator(),
            new SqliteMathAbsTranslator(),
            new SqliteStartsWithOptimizedTranslator(),
            new SqliteStringIsNullOrWhiteSpaceTranslator(),
            new SqliteStringToLowerTranslator(),
            new SqliteStringToUpperTranslator(),
            new SqliteStringTrimEndTranslator(),
            new SqliteStringTrimStartTranslator(),
            new SqliteStringTrimTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteCompositeMethodCallTranslator(
            [NotNull] ILogger<SqliteCompositeMethodCallTranslator> logger)
            : base(logger)
        {
            AddTranslators(_sqliteTranslators);
        }
    }
}
