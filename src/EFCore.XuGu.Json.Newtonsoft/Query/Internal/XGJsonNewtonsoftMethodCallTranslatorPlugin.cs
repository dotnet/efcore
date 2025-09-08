// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Query.Internal
{
    public class XGJsonNewtonsoftMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public XGJsonNewtonsoftMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGJsonPocoTranslator jsonPocoTranslator)
        {
            var xgSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            var xgJsonPocoTranslator = (XGJsonPocoTranslator)jsonPocoTranslator;

            Translators = new IMethodCallTranslator[]
            {
                new XGJsonNewtonsoftDomTranslator(
                    xgSqlExpressionFactory,
                    typeMappingSource,
                    xgJsonPocoTranslator),
            };
        }

        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
