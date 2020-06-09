// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerMethodCallTranslatorProvider([NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;
            var typeMappingSource = dependencies.RelationalTypeMappingSource;
            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new SqlServerByteArrayMethodTranslator(sqlExpressionFactory),
                    new SqlServerConvertTranslator(sqlExpressionFactory),
                    new SqlServerDataLengthFunctionTranslator(sqlExpressionFactory),
                    new SqlServerDateDiffFunctionsTranslator(sqlExpressionFactory),
                    new SqlServerDateTimeMethodTranslator(sqlExpressionFactory),
                    new SqlServerFromPartsFunctionTranslator(sqlExpressionFactory, typeMappingSource),
                    new SqlServerFullTextSearchFunctionsTranslator(sqlExpressionFactory),
                    new SqlServerIsDateFunctionTranslator(sqlExpressionFactory),
                    new SqlServerMathTranslator(sqlExpressionFactory),
                    new SqlServerNewGuidTranslator(sqlExpressionFactory),
                    new SqlServerObjectToStringTranslator(sqlExpressionFactory),
                    new SqlServerStringMethodTranslator(sqlExpressionFactory)
                });
        }
    }
}
