// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public SqliteMethodCallTranslatorProvider(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor,
            IEnumerable<IMethodCallTranslatorPlugin> plugins)
            : base(typeMappingSource, typeMappingApplyingExpressionVisitor, plugins)
        {
            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new SqliteMathTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                    new SqliteDateTimeAddTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                    new SqliteStringMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                });
        }
    }
}
