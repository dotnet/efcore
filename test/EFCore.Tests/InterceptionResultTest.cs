// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InterceptionResultTest
    {
        [ConditionalFact]
        public void InterceptionResult_Result_throws_if_HasResult_is_false()
        {
            Assert.Equal(
                CoreStrings.NoInterceptionResult,
                Assert.Throws<InvalidOperationException>(
                    () => default(InterceptionResult<object>).Result).Message);
        }
    }
}
