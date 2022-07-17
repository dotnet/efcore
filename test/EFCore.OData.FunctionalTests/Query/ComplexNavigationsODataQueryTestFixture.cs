// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsODataQueryTestFixture : ComplexNavigationsQuerySqlServerFixture, IODataQueryTestFixture
{
    private IHost _selfHostServer;

    protected override string StoreName
        => "ODataComplexNavigations";

    public ComplexNavigationsODataQueryTestFixture()
    {
        (BaseAddress, ClientFactory, _selfHostServer)
            = ODataQueryTestFixtureInitializer.Initialize<ComplexNavigationsODataContext>(StoreName, GetEdmModel());
    }

    private static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<Level1>("LevelOne");
        modelBuilder.EntitySet<Level2>("LevelTwo");
        modelBuilder.EntitySet<Level3>("LevelThree");
        modelBuilder.EntitySet<Level4>("LevelFour");

        return modelBuilder.GetEdmModel();
    }

    public string BaseAddress { get; }

    public IHttpClientFactory ClientFactory { get; }

    public override async Task DisposeAsync()
    {
        if (_selfHostServer != null)
        {
            await _selfHostServer.StopAsync();
            _selfHostServer.Dispose();
            _selfHostServer = null;
        }
    }
}
