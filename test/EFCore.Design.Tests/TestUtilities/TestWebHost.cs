// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestWebHost
    {
        public TestWebHost(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
