// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class DbContextAttributeTest
{
    [ConditionalFact]
    public void Create_attribute()
    {
        var attribute = new DbContextAttribute(typeof(MyContext));

        Assert.Same(typeof(MyContext), attribute.ContextType);
    }

    public class MyContext : DbContext;
}
