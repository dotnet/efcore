// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Data.Entity.SqlServer.Query.Methods;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _sqlServerTranslators = new List<IMethodCallTranslator>
        {
            new NewGuidTranslator(),
            new StringSubstringTranslator(),
            new MathAbsTranslator(),
            new MathCeilingTranslator(),
            new MathFloorTranslator(),
            new MathPowerTranslator(),
            new MathRoundTranslator(),
            new MathTruncateTranslator(),
            new StringReplaceTranslator(),
            new StringToLowerTranslator(),
            new StringToUpperTranslator()
        };

        public SqlServerCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        protected override IReadOnlyList<IMethodCallTranslator> Translators
            => base.Translators.Concat(_sqlServerTranslators).ToList();
    }
}
