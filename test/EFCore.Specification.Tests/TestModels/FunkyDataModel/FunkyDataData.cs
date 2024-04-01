// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

#nullable disable

public class FunkyDataData : ISetSource
{
    public static readonly FunkyDataData Instance = new();

    public IReadOnlyList<FunkyCustomer> FunkyCustomers { get; }

    private FunkyDataData()
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
            new()
            {
                Id = 1,
                FirstName = "%Bar",
                LastName = "%B",
                NullableBool = true
            },
            new()
            {
                Id = 2,
                FirstName = "Ba%r",
                LastName = "a%",
                NullableBool = true
            },
            new()
            {
                Id = 3,
                FirstName = "Bar%",
                LastName = "%B%",
                NullableBool = true
            },
            new()
            {
                Id = 4,
                FirstName = "%Ba%r%",
                LastName = null,
                NullableBool = false
            },
            new()
            {
                Id = 5,
                FirstName = "B%a%%r%",
                LastName = "r%",
                NullableBool = false
            },
            new()
            {
                Id = 6,
                FirstName = null,
                LastName = "%B%a%r"
            },
            new()
            {
                Id = 7,
                FirstName = "%B%a%r",
                LastName = ""
            },
            new()
            {
                Id = 8,
                FirstName = "",
                LastName = "%%r%"
            },
            new()
            {
                Id = 9,
                FirstName = "_Bar",
                LastName = "_B",
                NullableBool = false
            },
            new()
            {
                Id = 10,
                FirstName = "Ba_r",
                LastName = "a_",
                NullableBool = false
            },
            new()
            {
                Id = 11,
                FirstName = "Bar_",
                LastName = "_B_",
                NullableBool = false
            },
            new()
            {
                Id = 12,
                FirstName = "_Ba_r_",
                LastName = null,
                NullableBool = true
            },
            new()
            {
                Id = 13,
                FirstName = "B_a__r_",
                LastName = "r_",
                NullableBool = true
            },
            new()
            {
                Id = 14,
                FirstName = null,
                LastName = "_B_a_r"
            },
            new()
            {
                Id = 15,
                FirstName = "_B_a_r",
                LastName = ""
            },
            new()
            {
                Id = 16,
                FirstName = "",
                LastName = "__r_"
            },
            new()
            {
                Id = 17,
                FirstName = "[]Bar",
                LastName = "[]",
                NullableBool = true
            },
            new()
            {
                Id = 18,
                FirstName = "B[a]r",
                LastName = "B[",
                NullableBool = true
            },
            new()
            {
                Id = 19,
                FirstName = "B[[a^r",
                LastName = "B[[",
                NullableBool = true
            }
        };
}
