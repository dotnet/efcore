// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    // issue #15264
    internal class FiltersInheritanceSqliteTest : FiltersInheritanceTestBase<FiltersInheritanceSqliteFixture>
    {
        public FiltersInheritanceSqliteTest(FiltersInheritanceSqliteFixture fixture)
            : base(fixture)
        {
        }
    }
}
