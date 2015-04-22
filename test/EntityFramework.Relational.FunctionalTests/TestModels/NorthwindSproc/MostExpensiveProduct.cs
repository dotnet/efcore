// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational.FunctionalTests.TestModels.NorthwindSproc
{
    public class MostExpensiveProduct
    {
        public string TenMostExpensiveProducts { get; set; }

        public decimal? UnitPrice { get; set; }
    }
}
