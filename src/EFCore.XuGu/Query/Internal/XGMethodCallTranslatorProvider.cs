// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public XGMethodCallTranslatorProvider(
            [NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            var sqlExpressionFactory = (XGSqlExpressionFactory)dependencies.SqlExpressionFactory;
            var relationalTypeMappingSource = (XGTypeMappingSource)dependencies.RelationalTypeMappingSource;

            AddTranslators(new IMethodCallTranslator[]
            {
                new XGByteArrayMethodTranslator(sqlExpressionFactory),
                new XGConvertTranslator(sqlExpressionFactory),
                new XGDateTimeMethodTranslator(sqlExpressionFactory),
                new XGDateDiffFunctionsTranslator(sqlExpressionFactory),
                new XGDbFunctionsExtensionsMethodTranslator(sqlExpressionFactory),
                new XGJsonDbFunctionsTranslator(sqlExpressionFactory),
                new XGMathMethodTranslator(sqlExpressionFactory),
                new XGNewGuidTranslator(sqlExpressionFactory),
                new XGObjectToStringTranslator(sqlExpressionFactory),
                new XGRegexIsMatchTranslator(sqlExpressionFactory),
                new XGStringComparisonMethodTranslator(sqlExpressionFactory, () => QueryCompilationContext, options),
                new XGStringMethodTranslator(sqlExpressionFactory, relationalTypeMappingSource, () => QueryCompilationContext, options),
            });
        }

        public virtual QueryCompilationContext QueryCompilationContext { get; set; }

        public override SqlExpression Translate(
            IModel model,
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => QueryCompilationContext is not null
                ? base.Translate(model, instance, method, arguments, logger)
                : throw new InvalidOperationException();
    }
}
