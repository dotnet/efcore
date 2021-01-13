// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class EFTest
    {
        [ConditionalFact]
        public void Property_throws_when_invoked_outside_of_query()
        {
            Assert.Equal(
                CoreStrings.PropertyMethodInvoked,
                Assert.Throws<InvalidOperationException>(() => EF.Property<object>(new object(), "")).Message);
        }
    }
}
