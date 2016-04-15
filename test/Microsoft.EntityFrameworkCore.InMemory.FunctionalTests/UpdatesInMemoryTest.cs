// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class UpdatesInMemoryTest : UpdatesTestBase<UpdatesInMemoryFixture, InMemoryTestStore>
    {
        public UpdatesInMemoryTest(UpdatesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override string UpdateConcurrencyMessage
            => InMemoryStrings.UpdateConcurrencyException;
    }
}
