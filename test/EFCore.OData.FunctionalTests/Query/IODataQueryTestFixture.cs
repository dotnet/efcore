// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public interface IODataQueryTestFixture
{
    public string BaseAddress { get; }

    public IHttpClientFactory ClientFactory { get; }
}
