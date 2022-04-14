// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ODataQueryTestBase
{
    public ODataQueryTestBase(IODataQueryTestFixture fixture)
    {
        BaseAddress = fixture.BaseAddress;
        Client = fixture.ClientFactory.CreateClient();
    }

    public string BaseAddress { get; }

    public HttpClient Client { get; }
}
