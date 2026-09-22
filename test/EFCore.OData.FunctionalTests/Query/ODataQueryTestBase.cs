// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ODataQueryTestBase(IODataQueryTestFixture fixture)
{
    public string BaseAddress { get; } = fixture.BaseAddress;

    public HttpClient Client { get; } = fixture.ClientFactory.CreateClient();
}
