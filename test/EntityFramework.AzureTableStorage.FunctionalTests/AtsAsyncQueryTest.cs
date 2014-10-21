// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsAsyncQueryTest : AsyncQueryTestBase<AtsNorthwindQueryFixture>
    {
        public AtsAsyncQueryTest(AtsNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
