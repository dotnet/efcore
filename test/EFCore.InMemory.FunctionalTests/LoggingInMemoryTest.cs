// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class LoggingInMemoryTest : LoggingTestBase
    {
        protected override DbContextOptionsBuilder CreateOptionsBuilder()
            => new DbContextOptionsBuilder().UseInMemoryDatabase("LoggingInMemoryTest");

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.InMemory";

        protected override string DefaultOptions => "StoreName=LoggingInMemoryTest ";
    }
}
