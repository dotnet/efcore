// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public class DatabindingSqlServerTest : DatabindingTestBase<SqlServerTestStore, F1SqlServerFixture>
    {
        public DatabindingSqlServerTest(F1SqlServerFixture fixture)
            : base(fixture)
        {
        }
    }
}
