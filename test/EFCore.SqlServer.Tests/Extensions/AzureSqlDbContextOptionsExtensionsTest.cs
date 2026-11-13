// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class AzureSqlDbContextOptionsExtensionsTest
{
    [ConditionalFact]
    public void Can_call_UseNetTopologySuite_with_UseAzureSql()
    {
        // This test just makes sure we can call/compile UseNetTopologySuite with UseAzureSql.
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseAzureSql("Database=Crunchie", b => b.UseNetTopologySuite());
    }
}
