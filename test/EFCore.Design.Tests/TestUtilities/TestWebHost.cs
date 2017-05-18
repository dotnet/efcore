// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestWebHost
    {
        public TestWebHost(IServiceProvider services)
            => Services = services;

        public IServiceProvider Services { get; }
    }
}
