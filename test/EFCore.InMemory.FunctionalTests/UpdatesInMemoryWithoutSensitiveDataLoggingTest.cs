// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryWithoutSensitiveDataLoggingTest : UpdatesInMemoryTestBase<UpdatesInMemoryWithoutSensitiveDataLoggingFixture>
    {
        public UpdatesInMemoryWithoutSensitiveDataLoggingTest(UpdatesInMemoryWithoutSensitiveDataLoggingFixture fixture)
            : base(fixture)
        {
        }

        protected override string UpdateConcurrencyTokenMessage
            => InMemoryStrings.UpdateConcurrencyTokenException("Product", "{'Price'}");
    }
}
