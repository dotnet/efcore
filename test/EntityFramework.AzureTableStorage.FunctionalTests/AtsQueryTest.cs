// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsQueryTest : QueryTestBase<AtsNorthwindQueryFixture>
    {
        public AtsQueryTest(AtsNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
