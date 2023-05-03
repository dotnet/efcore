// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class DesignTimeServicesTests
{
    [ConditionalFact]
    public void ConfigureDesignTimeServices_works()
    {
        var serviceCollection = new ServiceCollection();
        new SqlServerHierarchyIdDesignTimeServices().ConfigureDesignTimeServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        Assert.IsType<SqlServerHierarchyIdTypeMappingSourcePlugin>(serviceProvider.GetService<IRelationalTypeMappingSourcePlugin>());
        Assert.IsType<SqlServerHierarchyIdCodeGeneratorPlugin>(serviceProvider.GetService<IProviderCodeGeneratorPlugin>());
    }
}
