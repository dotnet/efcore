// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Oracle.Query.ExpressionTranslators.Internal
{
    public class OracleCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        public OracleCompositeMemberTranslator([NotNull] RelationalCompositeMemberTranslatorDependencies dependencies)
            : base(dependencies)
        {
            var translators = new List<IMemberTranslator>
            {
                new OracleStringLengthTranslator(),
                new OracleDateTimeMemberTranslator(),
            };

            AddTranslators(translators);
        }
    }
}
