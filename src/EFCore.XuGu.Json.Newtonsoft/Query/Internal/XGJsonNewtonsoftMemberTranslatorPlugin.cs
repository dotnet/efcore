// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Query.Internal
{
    public class XGJsonNewtonsoftMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public XGJsonNewtonsoftMemberTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGJsonPocoTranslator jsonPocoTranslator)
        {
            var xgSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            var xgJsonPocoTranslator = (XGJsonPocoTranslator)jsonPocoTranslator;

            Translators = new IMemberTranslator[]
            {
                new XGJsonNewtonsoftDomTranslator(
                    xgSqlExpressionFactory,
                    typeMappingSource,
                    xgJsonPocoTranslator),
                jsonPocoTranslator,
            };
        }

        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }
}
