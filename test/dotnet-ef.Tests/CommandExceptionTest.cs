// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class CommandExceptionTest
{
    [Fact]
    public void Ctor_works()
    {
        var ex = new CommandException("Message1");

        Assert.Equal("Message1", ex.Message);
    }
}
