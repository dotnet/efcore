// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryGraphUpdatesTest : InMemoryGraphUpdatesTestBase<InMemoryGraphUpdatesTest.InMemoryGraphUpdatesFixture>
    {
        public InMemoryGraphUpdatesTest(InMemoryGraphUpdatesFixture fixture)
            : base(fixture)
        {
        }

        public class InMemoryGraphUpdatesFixture : InMemoryGraphUpdatesFixtureBase
        {
            public override int IntSentinel => 0;
        }
    }
}
