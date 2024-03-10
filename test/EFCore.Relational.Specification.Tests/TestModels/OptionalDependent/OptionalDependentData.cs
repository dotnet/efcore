// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

#nullable disable

public class OptionalDependentData : ISetSource
{
    public OptionalDependentData()
    {
        EntitiesAllOptional = CreateEntitiesAllOptional();
        EntitiesSomeRequired = CreateEntitiesSomeRequired();
    }

    public IReadOnlyList<OptionalDependentEntityAllOptional> EntitiesAllOptional { get; }
    public IReadOnlyList<OptionalDependentEntitySomeRequired> EntitiesSomeRequired { get; }

    public static IReadOnlyList<OptionalDependentEntityAllOptional> CreateEntitiesAllOptional()
    {
        var e1 = new OptionalDependentEntityAllOptional
        {
            Id = 1,
            Name = "op_e1",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "1",
                OpProp2 = 1,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "11", OpNested2 = 11 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(2001, 1, 1), OpNested1 = "1001", OpNested2 = 1001 }
            }
        };

        var e2 = new OptionalDependentEntityAllOptional
        {
            Id = 2,
            Name = "op_e2",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "2",
                OpProp2 = 2,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = 21 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(2002, 1, 1), OpNested1 = "2001", OpNested2 = 2001 }
            }
        };

        var e3 = new OptionalDependentEntityAllOptional
        {
            Id = 3,
            Name = "op_e3",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "3",
                OpProp2 = 3,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = null },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(2003, 1, 1), OpNested1 = "3001", OpNested2 = 3001 }
            }
        };

        var e4 = new OptionalDependentEntityAllOptional
        {
            Id = 4,
            Name = "op_e4",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "4",
                OpProp2 = 4,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = 41 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(2004, 1, 1), OpNested1 = null, OpNested2 = 4001 }
            }
        };

        var e5 = new OptionalDependentEntityAllOptional
        {
            Id = 5,
            Name = "op_e5",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "5",
                OpProp2 = 5,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = 51 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(2005, 1, 1), OpNested1 = null, OpNested2 = null }
            }
        };

        var e6 = new OptionalDependentEntityAllOptional
        {
            Id = 6,
            Name = "op_e6",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "6",
                OpProp2 = 6,
                OpNav1 = null,
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(2005, 1, 1), OpNested1 = null, OpNested2 = 6001 }
            }
        };

        var e7 = new OptionalDependentEntityAllOptional
        {
            Id = 7,
            Name = "op_e7",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "7",
                OpProp2 = 7,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = 71 },
                OpNav2 = null
            }
        };

        var e8 = new OptionalDependentEntityAllOptional
        {
            Id = 8,
            Name = "op_e8",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "8",
                OpProp2 = 8,
                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = null },
                OpNav2 = null
            }
        };

        var e9 = new OptionalDependentEntityAllOptional
        {
            Id = 9,
            Name = "op_e9",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "9",
                OpProp2 = 9,
                OpNav1 = null,
                OpNav2 = null
            }
        };

        var e10 = new OptionalDependentEntityAllOptional
        {
            Id = 10,
            Name = "op_e10",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = "10",
                OpProp2 = null,
                OpNav1 = null,
                OpNav2 = null
            }
        };

        var e11 = new OptionalDependentEntityAllOptional
        {
            Id = 11,
            Name = "op_e11",
            Json = new OptionalDependentJsonAllOptional
            {
                OpProp1 = null,
                OpProp2 = null,
                OpNav1 = null,
                OpNav2 = null
            }
        };

        var e12 = new OptionalDependentEntityAllOptional
        {
            Id = 12,
            Name = "op_e12",
            Json = null
        };

        return new List<OptionalDependentEntityAllOptional> { e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12 };
    }

    public static IReadOnlyList<OptionalDependentEntitySomeRequired> CreateEntitiesSomeRequired()
    {
        var e1 = new OptionalDependentEntitySomeRequired
        {
            Id = 1,
            Name = "req_e1",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 1.5,
                OpProp1 = "1",
                OpProp2 = 1,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "11", OpNested2 = 11 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4001, 1, 1), OpNested1 = "12", OpNested2 = 12 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "13", OpNested2 = 13 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5001, 1, 1), OpNested1 = "14", OpNested2 = 14 }
            }
        };

        var e2 = new OptionalDependentEntitySomeRequired
        {
            Id = 2,
            Name = "req_e2",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 2.5,
                OpProp1 = "2",
                OpProp2 = 2,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = 21 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4002, 1, 1), OpNested1 = "22", OpNested2 = 22 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "23", OpNested2 = 23 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5002, 1, 1), OpNested1 = "24", OpNested2 = 24 }
            }
        };

        var e3 = new OptionalDependentEntitySomeRequired
        {
            Id = 3,
            Name = "req_e3",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 3.5,
                OpProp1 = "3",
                OpProp2 = 3,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = null },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4003, 1, 1), OpNested1 = "32", OpNested2 = 32 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "33", OpNested2 = 33 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5003, 1, 1), OpNested1 = "34", OpNested2 = 34 }
            }
        };

        var e4 = new OptionalDependentEntitySomeRequired
        {
            Id = 4,
            Name = "req_e4",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 4.5,
                OpProp1 = "4",
                OpProp2 = 4,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "41", OpNested2 = 41 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4004, 1, 1), OpNested1 = null, OpNested2 = null },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "43", OpNested2 = 43 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5004, 1, 1), OpNested1 = "44", OpNested2 = 44 }
            }
        };

        var e5 = new OptionalDependentEntitySomeRequired
        {
            Id = 5,
            Name = "req_e5",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 5.5,
                OpProp1 = "5",
                OpProp2 = 5,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "51", OpNested2 = 51 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4005, 1, 1), OpNested1 = "52", OpNested2 = 52 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = null },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5005, 1, 1), OpNested1 = "54", OpNested2 = 54 }
            }
        };

        var e6 = new OptionalDependentEntitySomeRequired
        {
            Id = 6,
            Name = "req_e6",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 6.5,
                OpProp1 = "6",
                OpProp2 = 6,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "61", OpNested2 = 61 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4006, 1, 1), OpNested1 = "62", OpNested2 = 62 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "63", OpNested2 = 63 },
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5006, 1, 1), OpNested1 = null, OpNested2 = null }
            }
        };

        var e7 = new OptionalDependentEntitySomeRequired
        {
            Id = 7,
            Name = "req_e7",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 7.5,
                OpProp1 = "7",
                OpProp2 = 7,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "71", OpNested2 = 71 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4007, 1, 1), OpNested1 = "72", OpNested2 = 72 },

                OpNav1 = null,
                OpNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = false, ReqNested2 = new DateTime(5007, 1, 1), OpNested1 = "74", OpNested2 = 74 }
            }
        };

        var e8 = new OptionalDependentEntitySomeRequired
        {
            Id = 8,
            Name = "req_e8",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 8.5,
                OpProp1 = "8",
                OpProp2 = 8,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "81", OpNested2 = 81 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4008, 1, 1), OpNested1 = "82", OpNested2 = 82 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "83", OpNested2 = 83 },
                OpNav2 = null
            }
        };

        var e9 = new OptionalDependentEntitySomeRequired
        {
            Id = 9,
            Name = "req_e9",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 9.5,
                OpProp1 = "9",
                OpProp2 = 9,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "91", OpNested2 = 91 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4009, 1, 1), OpNested1 = "92", OpNested2 = 92 },

                OpNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = null, OpNested2 = null },
                OpNav2 = null
            }
        };

        var e10 = new OptionalDependentEntitySomeRequired
        {
            Id = 10,
            Name = "req_e10",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 10.5,
                OpProp1 = "10",
                OpProp2 = 10,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "101", OpNested2 = 101 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4010, 1, 1), OpNested1 = "102", OpNested2 = 102 },

                OpNav1 = null,
                OpNav2 = null
            }
        };

        var e11 = new OptionalDependentEntitySomeRequired
        {
            Id = 11,
            Name = "req_e11",
            Json = new OptionalDependentJsonSomeRequired
            {
                ReqProp = 11.5,
                OpProp1 = null,
                OpProp2 = null,

                ReqNav1 = new OptionalDependentNestedJsonAllOptional { OpNested1 = "111", OpNested2 = 111 },
                ReqNav2 = new OptionalDependentNestedJsonSomeRequired { ReqNested1 = true, ReqNested2 = new DateTime(4011, 1, 1), OpNested1 = "112", OpNested2 = 112 },

                OpNav1 = null,
                OpNav2 = null
            }
        };

        var e12 = new OptionalDependentEntitySomeRequired
        {
            Id = 12,
            Name = "req_e12",
            Json = null
        };

        return new List<OptionalDependentEntitySomeRequired> { e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12 };
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(OptionalDependentEntityAllOptional))
        {
            return (IQueryable<TEntity>)EntitiesAllOptional.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OptionalDependentEntitySomeRequired))
        {
            return (IQueryable<TEntity>)EntitiesSomeRequired.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }
}
