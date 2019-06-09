// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class EFTest
    {
        [ConditionalFact]
        public void Property_throws_when_invoked_outside_of_query()
        {
            Assert.Equal(
                "The EF.Property<T> method may only be used within LINQ queries.",
                Assert.Throws<InvalidOperationException>(() => EF.Property<object>(new object(), "")).Message);
        }
    }
}
