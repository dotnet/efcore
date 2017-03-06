// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.FunkyDataModel
{
    public class FunkyDataContext : DbContext
    {
        public static readonly string StoreName = "FunkyData";

        public FunkyDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<FunkyCustomer> FunkyCustomers { get; set; }
    }
}
