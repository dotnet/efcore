// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Query.Internal
{
    public class XGJsonMicrosoftMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public XGJsonMicrosoftMemberTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGJsonPocoTranslator jsonPocoTranslator)
        {
            var xgSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            var xgJsonPocoTranslator = (XGJsonPocoTranslator)jsonPocoTranslator;

            Translators = new IMemberTranslator[]
            {
                new XGJsonMicrosoftDomTranslator(
                    xgSqlExpressionFactory,
                    typeMappingSource,
                    xgJsonPocoTranslator),
                jsonPocoTranslator,
            };
        }

        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }
}
