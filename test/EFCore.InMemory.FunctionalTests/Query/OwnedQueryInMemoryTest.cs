// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQueryInMemoryTest : OwnedQueryTestBase, IClassFixture<OwnedQueryInMemoryFixture>
    {
        private readonly OwnedQueryInMemoryFixture _fixture;

        public OwnedQueryInMemoryTest(OwnedQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();
    }
}
