// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
