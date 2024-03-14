// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

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
    public SqlServerMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
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
                new SqlServerDateOnlyMethodTranslator(sqlExpressionFactory),
                new SqlServerDateTimeMethodTranslator(sqlExpressionFactory, typeMappingSource),
                new SqlServerFromPartsFunctionTranslator(sqlExpressionFactory, typeMappingSource),
                new SqlServerFullTextSearchFunctionsTranslator(sqlExpressionFactory),
                new SqlServerIsDateFunctionTranslator(sqlExpressionFactory),
                new SqlServerIsNumericFunctionTranslator(sqlExpressionFactory),
                new SqlServerMathTranslator(sqlExpressionFactory),
                new SqlServerNewGuidTranslator(sqlExpressionFactory),
                new SqlServerObjectToStringTranslator(sqlExpressionFactory, typeMappingSource),
                new SqlServerStringMethodTranslator(sqlExpressionFactory),
                new SqlServerTimeOnlyMethodTranslator(sqlExpressionFactory)
            });
    }
}
