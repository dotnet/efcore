// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class EFTest
{
    [ConditionalFact]
    public void Property_throws_when_invoked_outside_of_query()
        => Assert.Equal(
            CoreStrings.PropertyMethodInvoked,
            Assert.Throws<InvalidOperationException>(() => EF.Property<object>(new object(), "")).Message);
}
