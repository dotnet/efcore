// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class NavigationsQuerySqliteTests : NavigationsQueryTests
    {
        protected override AdventureWorksContextBase CreateContext()
        {
            return AdventureWorksFixture.CreateContext();
        }

        public override Task PredicateAcrossOptionalNavigation()
        {
            return base.PredicateAcrossOptionalNavigation();
        }
    }
}
