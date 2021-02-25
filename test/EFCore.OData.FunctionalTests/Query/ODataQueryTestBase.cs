// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;

namespace Microsoft.EntityFrameworkCore.Query
{
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
}
