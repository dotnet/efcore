// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqlServerMethodCallTranslatorProvider(IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(typeMappingSource, typeMappingApplyingExpressionVisitor, plugins)
        {
            AddTranslators(new IMethodCallTranslator[]
            {
                new SqlServerMathTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlServerNewGuidTranslator(typeMappingSource),
                new SqlServerStringMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlServerDateTimeMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlServerDateDiffFunctionsTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlServerConvertTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlServerObjectToStringTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlServerFullTextSearchFunctionsTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
            });
        }
    }
}
