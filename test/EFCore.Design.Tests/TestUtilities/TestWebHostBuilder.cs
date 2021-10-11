// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestWebHostBuilder
    {
        public TestWebHostBuilder(IServiceProvider services)
            => Services = services;

        public IServiceProvider Services { get; }

        public TestWebHost Build()
            => new(Services);
    }
}
