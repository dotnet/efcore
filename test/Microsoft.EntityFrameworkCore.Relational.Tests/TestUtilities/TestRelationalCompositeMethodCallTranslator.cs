// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
{
    public class TestRelationalCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        public TestRelationalCompositeMethodCallTranslator(ILogger<TestRelationalCompositeMethodCallTranslator> logger)
            : base(logger)
        {
        }
    }
}
