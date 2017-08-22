// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class OracleCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        public OracleCompositeMemberTranslator([NotNull] RelationalCompositeMemberTranslatorDependencies dependencies)
            : base(dependencies)
        {
            var translators = new List<IMemberTranslator>
            {
                new OracleStringLengthTranslator(),
                new OracleDateTimeNowTranslator(),
                new OracleDateTimeDateComponentTranslator(),
                new OracleDateTimeDatePartComponentTranslator()
            };

            AddTranslators(translators);
        }
    }
}
