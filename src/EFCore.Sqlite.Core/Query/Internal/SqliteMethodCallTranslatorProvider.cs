// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
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
        public SqliteMethodCallTranslatorProvider([NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;

            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new SqliteByteArrayMethodTranslator(sqlExpressionFactory, dependencies.RelationalTypeMappingSource),
                    new SqliteCharMethodTranslator(sqlExpressionFactory),
                    new SqliteDateTimeAddTranslator(sqlExpressionFactory),
                    new SqliteGlobMethodTranslator(sqlExpressionFactory),
                    new SqliteHexMethodTranslator(sqlExpressionFactory),
                    new SqliteMathTranslator(sqlExpressionFactory),
                    new SqliteObjectToStringTranslator(sqlExpressionFactory),
                    new SqliteRegexMethodTranslator(sqlExpressionFactory),
                    new SqliteStringMethodTranslator(sqlExpressionFactory),
                    new SqliteSubstrMethodTranslator(sqlExpressionFactory)
                });
        }
    }
}
