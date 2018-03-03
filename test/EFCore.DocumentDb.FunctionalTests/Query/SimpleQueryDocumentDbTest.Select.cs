// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryDocumentDbTest : SimpleQueryTestBase<NorthwindQueryDocumentDbFixture<NoopModelCustomizer>>
    {

        public override void Select_into()
        {
            base.Select_into();
        }
    }
}
