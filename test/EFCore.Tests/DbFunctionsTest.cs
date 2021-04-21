// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore
{
    public class DbFunctionsTest
    {
        [ConditionalFact]
        public void Like_on_client_throws()
        {
            Assert.Equal(
                CoreStrings.FunctionOnClient(nameof(DbFunctionsExtensions.Like)),
                Assert.Throws<InvalidOperationException>(
                    () => EF.Functions.Like("abc", "abc")).Message);
        }
    }
}
