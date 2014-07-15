// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class EndToEndTests : TestBase, IClassFixture<TestFixture>, IDisposable
    {
        public EndToEndTests(TestFixture fixture)
        {
            TestPartition = "endtoendtests" + DateTime.UtcNow.ToBinary();
            Context = fixture.CreateContext(TestPartition);
            Context.Database.EnsureCreated();
            Context.Set<Purchase>().AddRange(TestFixture.SampleData(TestPartition));
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
        }
    }
}
