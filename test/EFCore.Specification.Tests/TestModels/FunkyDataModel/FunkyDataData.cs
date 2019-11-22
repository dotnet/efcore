// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel
{
    public class FunkyDataData : ISetSource
    {
        public IReadOnlyList<FunkyCustomer> FunkyCustomers { get; }

        public FunkyDataData()
        {
            FunkyCustomers = CreateFunkyCustomers();
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(FunkyCustomer))
            {
                return (IQueryable<TEntity>)FunkyCustomers.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static IReadOnlyList<FunkyCustomer> CreateFunkyCustomers()
            => new List<FunkyCustomer>
            {
                new FunkyCustomer
                {
                    Id = 1,
                    FirstName = "%Bar",
                    LastName = "%B",
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 2,
                    FirstName = "Ba%r",
                    LastName = "a%",
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 3,
                    FirstName = "Bar%",
                    LastName = "%B%",
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 4,
                    FirstName = "%Ba%r%",
                    LastName = null,
                    NullableBool = false
                },
                new FunkyCustomer
                {
                    Id = 5,
                    FirstName = "B%a%%r%",
                    LastName = "r%",
                    NullableBool = false
                },
                new FunkyCustomer
                {
                    Id = 6,
                    FirstName = null,
                    LastName = "%B%a%r"
                },
                new FunkyCustomer
                {
                    Id = 7,
                    FirstName = "%B%a%r",
                    LastName = ""
                },
                new FunkyCustomer
                {
                    Id = 8,
                    FirstName = "",
                    LastName = "%%r%"
                },
                new FunkyCustomer
                {
                    Id = 9,
                    FirstName = "_Bar",
                    LastName = "_B",
                    NullableBool = false
                },
                new FunkyCustomer
                {
                    Id = 10,
                    FirstName = "Ba_r",
                    LastName = "a_",
                    NullableBool = false
                },
                new FunkyCustomer
                {
                    Id = 11,
                    FirstName = "Bar_",
                    LastName = "_B_",
                    NullableBool = false
                },
                new FunkyCustomer
                {
                    Id = 12,
                    FirstName = "_Ba_r_",
                    LastName = null,
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 13,
                    FirstName = "B_a__r_",
                    LastName = "r_",
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 14,
                    FirstName = null,
                    LastName = "_B_a_r"
                },
                new FunkyCustomer
                {
                    Id = 15,
                    FirstName = "_B_a_r",
                    LastName = ""
                },
                new FunkyCustomer
                {
                    Id = 16,
                    FirstName = "",
                    LastName = "__r_"
                },
                new FunkyCustomer
                {
                    Id = 17,
                    FirstName = "[]Bar",
                    LastName = "[]",
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 18,
                    FirstName = "B[a]r",
                    LastName = "B[",
                    NullableBool = true
                },
                new FunkyCustomer
                {
                    Id = 19,
                    FirstName = "B[[a^r",
                    LastName = "B[[",
                    NullableBool = true
                }
            };
    }
}
