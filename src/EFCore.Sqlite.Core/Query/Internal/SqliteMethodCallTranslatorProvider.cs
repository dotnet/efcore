// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
        var sqlExpressionFactory = (SqliteSqlExpressionFactory)dependencies.SqlExpressionFactory;

        AddTranslators(
            new IMethodCallTranslator[]
            {
                new SqliteByteArrayMethodTranslator(sqlExpressionFactory),
                new SqliteCharMethodTranslator(sqlExpressionFactory),
                new SqliteDateOnlyMethodTranslator(sqlExpressionFactory),
                new SqliteDateTimeMethodTranslator(sqlExpressionFactory),
                new SqliteGlobMethodTranslator(sqlExpressionFactory),
                new SqliteHexMethodTranslator(sqlExpressionFactory),
                new SqliteMathTranslator(sqlExpressionFactory),
                new SqliteObjectToStringTranslator(sqlExpressionFactory),
                new SqliteRandomTranslator(sqlExpressionFactory),
                new SqliteRegexMethodTranslator(sqlExpressionFactory),
                new SqliteStringMethodTranslator(sqlExpressionFactory),
                new SqliteSubstrMethodTranslator(sqlExpressionFactory)
            });
    }
}
