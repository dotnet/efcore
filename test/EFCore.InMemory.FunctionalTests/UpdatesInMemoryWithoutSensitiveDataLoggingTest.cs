// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !Test20
using Microsoft.EntityFrameworkCore.InMemory.Internal;
#endif

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryWithoutSensitiveDataLoggingTest : UpdatesInMemoryTestBase<UpdatesInMemoryWithoutSensitiveDataLoggingFixture>
    {
        public UpdatesInMemoryWithoutSensitiveDataLoggingTest(UpdatesInMemoryWithoutSensitiveDataLoggingFixture fixture)
            : base(fixture)
        {
        }

#if !Test20
        protected override string UpdateConcurrencyTokenMessage
            => InMemoryStrings.UpdateConcurrencyTokenException("Product", "{'Price'}");
#endif
    }
}
