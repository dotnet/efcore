// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel
{
    public class FunkyDataContext : PoolableDbContext
    {
        public FunkyDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<FunkyCustomer> FunkyCustomers { get; set; }

        public static void Seed(FunkyDataContext context)
        {
            var c11 = new FunkyCustomer
            {
                FirstName = "%Bar",
                LastName = "%B",
                NullableBool = true
            };
            var c12 = new FunkyCustomer
            {
                FirstName = "Ba%r",
                LastName = "a%",
                NullableBool = true
            };
            var c13 = new FunkyCustomer
            {
                FirstName = "Bar%",
                LastName = "%B%",
                NullableBool = true
            };
            var c14 = new FunkyCustomer
            {
                FirstName = "%Ba%r%",
                LastName = null,
                NullableBool = false
            };
            var c15 = new FunkyCustomer
            {
                FirstName = "B%a%%r%",
                LastName = "r%",
                NullableBool = false
            };
            var c16 = new FunkyCustomer
            {
                FirstName = null,
                LastName = "%B%a%r"
            };
            var c17 = new FunkyCustomer
            {
                FirstName = "%B%a%r",
                LastName = ""
            };
            var c18 = new FunkyCustomer
            {
                FirstName = "",
                LastName = "%%r%"
            };

            var c21 = new FunkyCustomer
            {
                FirstName = "_Bar",
                LastName = "_B",
                NullableBool = false
            };
            var c22 = new FunkyCustomer
            {
                FirstName = "Ba_r",
                LastName = "a_",
                NullableBool = false
            };
            var c23 = new FunkyCustomer
            {
                FirstName = "Bar_",
                LastName = "_B_",
                NullableBool = false
            };
            var c24 = new FunkyCustomer
            {
                FirstName = "_Ba_r_",
                LastName = null,
                NullableBool = true
            };
            var c25 = new FunkyCustomer
            {
                FirstName = "B_a__r_",
                LastName = "r_",
                NullableBool = true
            };
            var c26 = new FunkyCustomer
            {
                FirstName = null,
                LastName = "_B_a_r"
            };
            var c27 = new FunkyCustomer
            {
                FirstName = "_B_a_r",
                LastName = ""
            };
            var c28 = new FunkyCustomer
            {
                FirstName = "",
                LastName = "__r_"
            };

            context.FunkyCustomers.AddRange(c11, c12, c13, c14, c15, c16, c17, c18, c21, c22, c23, c24, c25, c26, c27, c28);

            context.SaveChanges();
        }
    }
}
