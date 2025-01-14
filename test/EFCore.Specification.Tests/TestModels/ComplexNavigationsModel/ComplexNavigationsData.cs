// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

public abstract class ComplexNavigationsData : ISetSource
{
    public IReadOnlyList<Level1> LevelOnes { get; }
    public IReadOnlyList<Level2> LevelTwos { get; }
    public IReadOnlyList<Level3> LevelThrees { get; }
    public IReadOnlyList<Level4> LevelFours { get; }

    public IReadOnlyList<Level1> SplitLevelOnes { get; }
    public IReadOnlyList<Level2> SplitLevelTwos { get; }
    public IReadOnlyList<Level3> SplitLevelThrees { get; }
    public IReadOnlyList<Level4> SplitLevelFours { get; }

    public IReadOnlyList<InheritanceBase1> InheritanceBaseOnes { get; }
    public IReadOnlyList<InheritanceBase2> InheritanceBaseTwos { get; }
    public IReadOnlyList<InheritanceLeaf1> InheritanceLeafOnes { get; }
    public IReadOnlyList<InheritanceLeaf2> InheritanceLeafTwos { get; }

    public abstract IQueryable<TEntity> Set<TEntity>()
        where TEntity : class;

    protected ComplexNavigationsData()
    {
        LevelOnes = CreateLevelOnes(tableSplitting: false);
        LevelTwos = CreateLevelTwos(tableSplitting: false);
        LevelThrees = CreateLevelThrees(tableSplitting: false);
        LevelFours = CreateLevelFours(tableSplitting: false);

        WireUpPart1(LevelOnes, LevelTwos, LevelThrees, LevelFours, tableSplitting: false);
        WireUpInversePart1(LevelOnes, LevelTwos, LevelThrees, LevelFours, tableSplitting: false);

        WireUpPart2(LevelOnes, LevelTwos, LevelThrees, LevelFours, tableSplitting: false);
        WireUpInversePart2(LevelOnes, LevelTwos, LevelThrees, LevelFours, tableSplitting: false);

        SplitLevelOnes = CreateLevelOnes(tableSplitting: true);
        SplitLevelTwos = CreateLevelTwos(tableSplitting: true);
        SplitLevelThrees = CreateLevelThrees(tableSplitting: true);
        SplitLevelFours = CreateLevelFours(tableSplitting: true);

        WireUpPart1(SplitLevelOnes, SplitLevelTwos, SplitLevelThrees, SplitLevelFours, tableSplitting: true);
        WireUpInversePart1(SplitLevelOnes, SplitLevelTwos, SplitLevelThrees, SplitLevelFours, tableSplitting: true);

        WireUpPart2(SplitLevelOnes, SplitLevelTwos, SplitLevelThrees, SplitLevelFours, tableSplitting: true);
        WireUpInversePart2(SplitLevelOnes, SplitLevelTwos, SplitLevelThrees, SplitLevelFours, tableSplitting: true);

        InheritanceBaseOnes = CreateInheritanceBaseOnes();
        InheritanceBaseTwos = CreateInheritanceBaseTwos();
        InheritanceLeafOnes = CreateInheritanceLeafOnes();
        InheritanceLeafTwos = CreateInheritanceLeafTwos();

        WireUpInheritancePart1(InheritanceBaseOnes, InheritanceBaseTwos, InheritanceLeafOnes, InheritanceLeafTwos);
        WireUpInheritancePart2(InheritanceBaseTwos, InheritanceLeafTwos);
    }

    public static IReadOnlyList<Level1> CreateLevelOnes(bool tableSplitting)
    {
        var result = new List<Level1>
        {
            new()
            {
                Id = 1,
                Name = "L1 01",
                Date = new DateTime(2001, 1, 1)
            },
            new()
            {
                Id = 2,
                Name = "L1 02",
                Date = new DateTime(2002, 2, 2)
            },
            new()
            {
                Id = 3,
                Name = "L1 03",
                Date = new DateTime(2003, 3, 3)
            },
            new()
            {
                Id = 4,
                Name = "L1 04",
                Date = new DateTime(2004, 4, 4)
            },
            new()
            {
                Id = 5,
                Name = "L1 05",
                Date = new DateTime(2005, 5, 5)
            },
            new()
            {
                Id = 6,
                Name = "L1 06",
                Date = new DateTime(2006, 6, 6)
            },
            new()
            {
                Id = 7,
                Name = "L1 07",
                Date = new DateTime(2007, 7, 7)
            },
            new()
            {
                Id = 8,
                Name = "L1 08",
                Date = new DateTime(2008, 8, 8)
            },
            new()
            {
                Id = 9,
                Name = "L1 09",
                Date = new DateTime(2009, 9, 9)
            },
            new()
            {
                Id = 10,
                Name = "L1 10",
                Date = new DateTime(2010, 10, 10)
            }
        };

        if (!tableSplitting)
        {
            result.AddRange(
                new List<Level1>
                {
                    new()
                    {
                        Id = 11,
                        Name = "L1 11",
                        Date = new DateTime(2009, 11, 11)
                    },
                    new()
                    {
                        Id = 12,
                        Name = "L1 12",
                        Date = new DateTime(2008, 12, 12)
                    },
                    new()
                    {
                        Id = 13,
                        Name = "L1 13",
                        Date = new DateTime(2007, 1, 1)
                    }
                });
        }

        foreach (var l1 in result)
        {
            l1.OneToMany_Optional1 = new List<Level2>();
            l1.OneToMany_Optional_Self1 = new List<Level1>();
            l1.OneToMany_Required1 = new List<Level2>();
            l1.OneToMany_Required_Self1 = new List<Level1>();
        }

        return result;
    }

    public static IReadOnlyList<Level2> CreateLevelTwos(bool tableSplitting)
    {
        var result = new List<Level2>
        {
            new()
            {
                Id = 1,
                Name = "L2 01",
                Date = new DateTime(2010, 10, 10)
            },
            new()
            {
                Id = 2,
                Name = "L2 02",
                Date = new DateTime(2002, 2, 2)
            },
            new()
            {
                Id = 3,
                Name = "L2 03",
                Date = new DateTime(2008, 8, 8)
            },
            new()
            {
                Id = 4,
                Name = "L2 04",
                Date = new DateTime(2004, 4, 4)
            },
            new()
            {
                Id = 5,
                Name = "L2 05",
                Date = new DateTime(2006, 6, 6)
            },
            new()
            {
                Id = 6,
                Name = "L2 06",
                Date = new DateTime(2005, 5, 5)
            },
            new()
            {
                Id = 7,
                Name = "L2 07",
                Date = new DateTime(2007, 7, 7)
            },
            new()
            {
                Id = 8,
                Name = "L2 08",
                Date = new DateTime(2003, 3, 3)
            },
            new()
            {
                Id = 9,
                Name = "L2 09",
                Date = new DateTime(2009, 9, 9)
            },
            new()
            {
                Id = 10,
                Name = "L2 10",
                Date = new DateTime(2001, 1, 1)
            }
        };

        if (!tableSplitting)
        {
            result.AddRange(
                new List<Level2>
                {
                    new()
                    {
                        Id = 11,
                        Name = "L2 11",
                        Date = new DateTime(2000, 1, 1)
                    }
                });
        }

        foreach (var l2 in result)
        {
            l2.OneToMany_Optional2 = new List<Level3>();
            l2.OneToMany_Optional_Self2 = new List<Level2>();
            l2.OneToMany_Required2 = new List<Level3>();
            l2.OneToMany_Required_Self2 = new List<Level2>();
        }

        return result;
    }

    public static IReadOnlyList<Level3> CreateLevelThrees(bool tableSplitting)
    {
        var result = new List<Level3>
        {
            new() { Id = 1, Name = "L3 01" },
            new() { Id = 2, Name = "L3 02" },
            new() { Id = 3, Name = "L3 03" },
            new() { Id = 4, Name = "L3 04" },
            new() { Id = 5, Name = "L3 05" },
            new() { Id = 6, Name = "L3 06" },
            new() { Id = 7, Name = "L3 07" },
            new() { Id = 8, Name = "L3 08" },
            new() { Id = 9, Name = "L3 09" },
            new() { Id = 10, Name = "L3 10" }
        };

        foreach (var l3 in result)
        {
            l3.OneToMany_Optional3 = new List<Level4>();
            l3.OneToMany_Optional_Self3 = new List<Level3>();
            l3.OneToMany_Required3 = new List<Level4>();
            l3.OneToMany_Required_Self3 = new List<Level3>();
        }

        return result;
    }

    public static IReadOnlyList<Level4> CreateLevelFours(bool tableSplitting)
    {
        var result = new List<Level4>
        {
            new() { Id = 1, Name = "L4 01" },
            new() { Id = 2, Name = "L4 02" },
            new() { Id = 3, Name = "L4 03" },
            new() { Id = 4, Name = "L4 04" },
            new() { Id = 5, Name = "L4 05" },
            new() { Id = 6, Name = "L4 06" },
            new() { Id = 7, Name = "L4 07" },
            new() { Id = 8, Name = "L4 08" },
            new() { Id = 9, Name = "L4 09" },
            new() { Id = 10, Name = "L4 10" }
        };

        foreach (var l4 in result)
        {
            l4.OneToMany_Optional_Self4 = new List<Level4>();
            l4.OneToMany_Required_Self4 = new List<Level4>();
        }

        return result;
    }

    public static IReadOnlyList<InheritanceBase1> CreateInheritanceBaseOnes()
    {
        var result = new List<InheritanceBase1>
        {
            new InheritanceDerived1 { Id = 1, Name = "ID1 01" },
            new InheritanceDerived1 { Id = 2, Name = "ID1 02" },
            new InheritanceDerived2 { Id = 3, Name = "ID2 01" }
        };

        return result;
    }

    public static IReadOnlyList<InheritanceBase2> CreateInheritanceBaseTwos()
    {
        var result = new List<InheritanceBase2> { new() { Id = 1, Name = "IB2 01" } };

        return result;
    }

    public static IReadOnlyList<InheritanceLeaf1> CreateInheritanceLeafOnes()
    {
        var result = new List<InheritanceLeaf1>
        {
            new() { Id = 1, Name = "IL1 01" },
            new() { Id = 2, Name = "IL1 02" },
            new() { Id = 3, Name = "IL1 03" }
        };

        return result;
    }

    public static IReadOnlyList<InheritanceLeaf2> CreateInheritanceLeafTwos()
    {
        var result = new List<InheritanceLeaf2> { new() { Id = 1, Name = "IL2 01" } };

        return result;
    }

    private static void WireUpInheritancePart1(
        IReadOnlyList<InheritanceBase1> ib1s,
        IReadOnlyList<InheritanceBase2> ib2s,
        IReadOnlyList<InheritanceLeaf1> il1s,
        IReadOnlyList<InheritanceLeaf2> il2s)
    {
        ib2s[0].Reference = ib1s[0];
        ib2s[0].Collection = [ib1s[1], ib1s[2]];

        ((InheritanceDerived1)ib1s[0]).ReferenceSameType = il1s[0];
        ((InheritanceDerived1)ib1s[1]).ReferenceSameType = il1s[1];
        ((InheritanceDerived2)ib1s[2]).ReferenceSameType = il1s[2];

        ((InheritanceDerived1)ib1s[0]).ReferenceDifferentType = il1s[0];
        ((InheritanceDerived1)ib1s[1]).ReferenceDifferentType = il1s[1];
        ((InheritanceDerived2)ib1s[2]).ReferenceDifferentType = il2s[0];

        ((InheritanceDerived1)ib1s[0]).CollectionSameType = [il1s[0]];
        ((InheritanceDerived1)ib1s[1]).CollectionSameType = [];
        ((InheritanceDerived2)ib1s[2]).CollectionSameType = [il1s[1], il1s[2]];

        ((InheritanceDerived1)ib1s[0]).CollectionDifferentType = [il1s[0]];
        ((InheritanceDerived1)ib1s[1]).CollectionDifferentType = [il1s[1], il1s[2]];
        ((InheritanceDerived2)ib1s[2]).CollectionDifferentType = [il2s[0]];
    }

    private static void WireUpInheritancePart2(
        IReadOnlyList<InheritanceBase2> ib2s,
        IReadOnlyList<InheritanceLeaf2> il2s)
        => il2s[0].BaseCollection = [ib2s[0]];

    private static void WireUpPart1(
        IReadOnlyList<Level1> l1s,
        IReadOnlyList<Level2> l2s,
        IReadOnlyList<Level3> l3s,
        IReadOnlyList<Level4> l4s,
        bool tableSplitting)
    {
        l1s[0].OneToOne_Required_PK1 = l2s[0];
        l1s[1].OneToOne_Required_PK1 = l2s[1];
        l1s[2].OneToOne_Required_PK1 = l2s[2];
        l1s[3].OneToOne_Required_PK1 = l2s[3];
        l1s[4].OneToOne_Required_PK1 = l2s[4];
        l1s[5].OneToOne_Required_PK1 = l2s[5];
        l1s[6].OneToOne_Required_PK1 = l2s[6];
        l1s[7].OneToOne_Required_PK1 = l2s[7];
        l1s[8].OneToOne_Required_PK1 = l2s[8];
        l1s[9].OneToOne_Required_PK1 = l2s[9];
        if (!tableSplitting)
        {
            l1s[10].OneToOne_Required_PK1 = l2s[10];
        }

        if (tableSplitting)
        {
            l1s[0].OneToOne_Required_FK1 = l2s[0];
            l1s[1].OneToOne_Required_FK1 = l2s[1];
            l1s[2].OneToOne_Required_FK1 = l2s[2];
            l1s[3].OneToOne_Required_FK1 = l2s[3];
            l1s[4].OneToOne_Required_FK1 = l2s[4];
            l1s[5].OneToOne_Required_FK1 = l2s[5];
            l1s[6].OneToOne_Required_FK1 = l2s[6];
            l1s[7].OneToOne_Required_FK1 = l2s[7];
            l1s[8].OneToOne_Required_FK1 = l2s[8];
            l1s[9].OneToOne_Required_FK1 = l2s[9];
        }
        else
        {
            l1s[0].OneToOne_Required_FK1 = l2s[9];
            l1s[1].OneToOne_Required_FK1 = l2s[8];
            l1s[2].OneToOne_Required_FK1 = l2s[7];
            l1s[3].OneToOne_Required_FK1 = l2s[6];
            l1s[4].OneToOne_Required_FK1 = l2s[5];
            l1s[5].OneToOne_Required_FK1 = l2s[4];
            l1s[6].OneToOne_Required_FK1 = l2s[3];
            l1s[7].OneToOne_Required_FK1 = l2s[2];
            l1s[8].OneToOne_Required_FK1 = l2s[1];
            l1s[9].OneToOne_Required_FK1 = l2s[0];
            l1s[10].OneToOne_Required_FK1 = l2s[10];
        }

        l1s[0].OneToMany_Required1 = new List<Level2>
        {
            l2s[0],
            l2s[1],
            l2s[2],
            l2s[3],
            l2s[4],
            l2s[5],
            l2s[6],
            l2s[7],
            l2s[8],
            l2s[9]
        };

        if (!tableSplitting)
        {
            l1s[0].OneToMany_Required1.Add(l2s[10]);
        }

        l1s[0].OneToMany_Required_Self1 = new List<Level1> { l1s[0], l1s[1] };
        if (!tableSplitting)
        {
            l1s[0].OneToMany_Required_Self1.Add(l1s[11]);
        }

        l1s[1].OneToMany_Required_Self1 = new List<Level1> { l1s[2] };
        if (!tableSplitting)
        {
            l1s[1].OneToMany_Required_Self1.Add(l1s[12]);
        }

        l1s[2].OneToMany_Required_Self1 = new List<Level1> { l1s[3] };
        l1s[3].OneToMany_Required_Self1 = new List<Level1> { l1s[4] };
        l1s[4].OneToMany_Required_Self1 = new List<Level1> { l1s[5] };
        l1s[5].OneToMany_Required_Self1 = new List<Level1> { l1s[6] };
        l1s[6].OneToMany_Required_Self1 = new List<Level1> { l1s[7] };
        l1s[7].OneToMany_Required_Self1 = new List<Level1> { l1s[8] };
        l1s[8].OneToMany_Required_Self1 = new List<Level1> { l1s[9] };
        l1s[9].OneToMany_Required_Self1 = new List<Level1>();
        if (!tableSplitting)
        {
            l1s[10].OneToMany_Required_Self1 = new List<Level1> { l1s[10] };
            l1s[11].OneToMany_Required_Self1 = new List<Level1>();
            l1s[12].OneToMany_Required_Self1 = new List<Level1>();
        }

        l2s[0].OneToOne_Required_PK2 = l3s[0];
        l2s[1].OneToOne_Required_PK2 = l3s[1];
        l2s[2].OneToOne_Required_PK2 = l3s[2];
        l2s[3].OneToOne_Required_PK2 = l3s[3];
        l2s[4].OneToOne_Required_PK2 = l3s[4];
        l2s[5].OneToOne_Required_PK2 = l3s[5];
        l2s[6].OneToOne_Required_PK2 = l3s[6];
        l2s[7].OneToOne_Required_PK2 = l3s[7];
        l2s[8].OneToOne_Required_PK2 = l3s[8];
        l2s[9].OneToOne_Required_PK2 = l3s[9];

        if (tableSplitting)
        {
            l2s[0].OneToOne_Required_FK2 = l3s[0];
            l2s[1].OneToOne_Required_FK2 = l3s[1];
            l2s[2].OneToOne_Required_FK2 = l3s[2];
            l2s[3].OneToOne_Required_FK2 = l3s[3];
            l2s[4].OneToOne_Required_FK2 = l3s[4];
            l2s[5].OneToOne_Required_FK2 = l3s[5];
            l2s[6].OneToOne_Required_FK2 = l3s[6];
            l2s[7].OneToOne_Required_FK2 = l3s[7];
            l2s[8].OneToOne_Required_FK2 = l3s[8];
            l2s[9].OneToOne_Required_FK2 = l3s[9];
        }
        else
        {
            l2s[0].OneToOne_Required_FK2 = l3s[9];
            l2s[1].OneToOne_Required_FK2 = l3s[8];
            l2s[2].OneToOne_Required_FK2 = l3s[7];
            l2s[3].OneToOne_Required_FK2 = l3s[6];
            l2s[4].OneToOne_Required_FK2 = l3s[5];
            l2s[5].OneToOne_Required_FK2 = l3s[4];
            l2s[6].OneToOne_Required_FK2 = l3s[3];
            l2s[7].OneToOne_Required_FK2 = l3s[2];
            l2s[8].OneToOne_Required_FK2 = l3s[1];
            l2s[9].OneToOne_Required_FK2 = l3s[0];
        }

        l2s[0].OneToMany_Required2 = new List<Level3>
        {
            l3s[0],
            l3s[1],
            l3s[2],
            l3s[3],
            l3s[4],
            l3s[5],
            l3s[6],
            l3s[7],
            l3s[8],
            l3s[9]
        };

        l2s[0].OneToMany_Required_Self2 = new List<Level2> { l2s[0], l2s[1] };
        if (!tableSplitting)
        {
            l2s[0].OneToMany_Required_Self2.Add(l2s[10]);
        }

        l2s[1].OneToMany_Required_Self2 = new List<Level2> { l2s[2] };
        l2s[2].OneToMany_Required_Self2 = new List<Level2> { l2s[3] };
        l2s[3].OneToMany_Required_Self2 = new List<Level2> { l2s[4] };
        l2s[4].OneToMany_Required_Self2 = new List<Level2> { l2s[5] };
        l2s[5].OneToMany_Required_Self2 = new List<Level2> { l2s[6] };
        l2s[6].OneToMany_Required_Self2 = new List<Level2> { l2s[7] };
        l2s[7].OneToMany_Required_Self2 = new List<Level2> { l2s[8] };
        l2s[8].OneToMany_Required_Self2 = new List<Level2> { l2s[9] };
        l2s[9].OneToMany_Required_Self2 = new List<Level2>();
        if (!tableSplitting)
        {
            l2s[10].OneToMany_Required_Self2 = new List<Level2>();
        }

        l3s[0].OneToOne_Required_PK3 = l4s[0];
        l3s[1].OneToOne_Required_PK3 = l4s[1];
        l3s[2].OneToOne_Required_PK3 = l4s[2];
        l3s[3].OneToOne_Required_PK3 = l4s[3];
        l3s[4].OneToOne_Required_PK3 = l4s[4];
        l3s[5].OneToOne_Required_PK3 = l4s[5];
        l3s[6].OneToOne_Required_PK3 = l4s[6];
        l3s[7].OneToOne_Required_PK3 = l4s[7];
        l3s[8].OneToOne_Required_PK3 = l4s[8];
        l3s[9].OneToOne_Required_PK3 = l4s[9];

        if (tableSplitting)
        {
            l3s[0].OneToOne_Required_FK3 = l4s[0];
            l3s[1].OneToOne_Required_FK3 = l4s[1];
            l3s[2].OneToOne_Required_FK3 = l4s[2];
            l3s[3].OneToOne_Required_FK3 = l4s[3];
            l3s[4].OneToOne_Required_FK3 = l4s[4];
            l3s[5].OneToOne_Required_FK3 = l4s[5];
            l3s[6].OneToOne_Required_FK3 = l4s[6];
            l3s[7].OneToOne_Required_FK3 = l4s[7];
            l3s[8].OneToOne_Required_FK3 = l4s[8];
            l3s[9].OneToOne_Required_FK3 = l4s[9];
        }
        else
        {
            l3s[0].OneToOne_Required_FK3 = l4s[9];
            l3s[1].OneToOne_Required_FK3 = l4s[8];
            l3s[2].OneToOne_Required_FK3 = l4s[7];
            l3s[3].OneToOne_Required_FK3 = l4s[6];
            l3s[4].OneToOne_Required_FK3 = l4s[5];
            l3s[5].OneToOne_Required_FK3 = l4s[4];
            l3s[6].OneToOne_Required_FK3 = l4s[3];
            l3s[7].OneToOne_Required_FK3 = l4s[2];
            l3s[8].OneToOne_Required_FK3 = l4s[1];
            l3s[9].OneToOne_Required_FK3 = l4s[0];
        }

        l3s[0].OneToMany_Required3 = new List<Level4>
        {
            l4s[0],
            l4s[1],
            l4s[2],
            l4s[3],
            l4s[4],
            l4s[5],
            l4s[6],
            l4s[7],
            l4s[8],
            l4s[9]
        };

        l3s[0].OneToMany_Required_Self3 = new List<Level3> { l3s[0], l3s[1] };
        l3s[1].OneToMany_Required_Self3 = new List<Level3> { l3s[2] };
        l3s[2].OneToMany_Required_Self3 = new List<Level3> { l3s[3] };
        l3s[3].OneToMany_Required_Self3 = new List<Level3> { l3s[4] };
        l3s[4].OneToMany_Required_Self3 = new List<Level3> { l3s[5] };
        l3s[5].OneToMany_Required_Self3 = new List<Level3> { l3s[6] };
        l3s[6].OneToMany_Required_Self3 = new List<Level3> { l3s[7] };
        l3s[7].OneToMany_Required_Self3 = new List<Level3> { l3s[8] };
        l3s[8].OneToMany_Required_Self3 = new List<Level3> { l3s[9] };
        l3s[9].OneToMany_Required_Self3 = new List<Level3>();

        l4s[0].OneToMany_Required_Self4 = new List<Level4> { l4s[0], l4s[1] };
        l4s[1].OneToMany_Required_Self4 = new List<Level4> { l4s[2] };
        l4s[2].OneToMany_Required_Self4 = new List<Level4> { l4s[3] };
        l4s[3].OneToMany_Required_Self4 = new List<Level4> { l4s[4] };
        l4s[4].OneToMany_Required_Self4 = new List<Level4> { l4s[5] };
        l4s[5].OneToMany_Required_Self4 = new List<Level4> { l4s[6] };
        l4s[6].OneToMany_Required_Self4 = new List<Level4> { l4s[7] };
        l4s[7].OneToMany_Required_Self4 = new List<Level4> { l4s[8] };
        l4s[8].OneToMany_Required_Self4 = new List<Level4> { l4s[9] };
        l4s[9].OneToMany_Required_Self4 = new List<Level4>();
    }

    private static void WireUpInversePart1(
        IReadOnlyList<Level1> l1s,
        IReadOnlyList<Level2> l2s,
        IReadOnlyList<Level3> l3s,
        IReadOnlyList<Level4> l4s,
        bool tableSplitting)
    {
        l2s[0].OneToOne_Required_PK_Inverse2 = l1s[0];
        l2s[1].OneToOne_Required_PK_Inverse2 = l1s[1];
        l2s[2].OneToOne_Required_PK_Inverse2 = l1s[2];
        l2s[3].OneToOne_Required_PK_Inverse2 = l1s[3];
        l2s[4].OneToOne_Required_PK_Inverse2 = l1s[4];
        l2s[5].OneToOne_Required_PK_Inverse2 = l1s[5];
        l2s[6].OneToOne_Required_PK_Inverse2 = l1s[6];
        l2s[7].OneToOne_Required_PK_Inverse2 = l1s[7];
        l2s[8].OneToOne_Required_PK_Inverse2 = l1s[8];
        l2s[9].OneToOne_Required_PK_Inverse2 = l1s[9];
        if (!tableSplitting)
        {
            l2s[10].OneToOne_Required_PK_Inverse2 = l1s[10];
        }

        if (tableSplitting)
        {
            l2s[0].OneToOne_Required_FK_Inverse2 = l1s[0];
            l2s[1].OneToOne_Required_FK_Inverse2 = l1s[1];
            l2s[2].OneToOne_Required_FK_Inverse2 = l1s[2];
            l2s[3].OneToOne_Required_FK_Inverse2 = l1s[3];
            l2s[4].OneToOne_Required_FK_Inverse2 = l1s[4];
            l2s[5].OneToOne_Required_FK_Inverse2 = l1s[5];
            l2s[6].OneToOne_Required_FK_Inverse2 = l1s[6];
            l2s[7].OneToOne_Required_FK_Inverse2 = l1s[7];
            l2s[8].OneToOne_Required_FK_Inverse2 = l1s[8];
            l2s[9].OneToOne_Required_FK_Inverse2 = l1s[9];

            l2s[0].Level1_Required_Id = l1s[0].Id;
            l2s[1].Level1_Required_Id = l1s[1].Id;
            l2s[2].Level1_Required_Id = l1s[2].Id;
            l2s[3].Level1_Required_Id = l1s[3].Id;
            l2s[4].Level1_Required_Id = l1s[4].Id;
            l2s[5].Level1_Required_Id = l1s[5].Id;
            l2s[6].Level1_Required_Id = l1s[6].Id;
            l2s[7].Level1_Required_Id = l1s[7].Id;
            l2s[8].Level1_Required_Id = l1s[8].Id;
            l2s[9].Level1_Required_Id = l1s[9].Id;
        }
        else
        {
            l2s[9].OneToOne_Required_FK_Inverse2 = l1s[0];
            l2s[8].OneToOne_Required_FK_Inverse2 = l1s[1];
            l2s[7].OneToOne_Required_FK_Inverse2 = l1s[2];
            l2s[6].OneToOne_Required_FK_Inverse2 = l1s[3];
            l2s[5].OneToOne_Required_FK_Inverse2 = l1s[4];
            l2s[4].OneToOne_Required_FK_Inverse2 = l1s[5];
            l2s[3].OneToOne_Required_FK_Inverse2 = l1s[6];
            l2s[2].OneToOne_Required_FK_Inverse2 = l1s[7];
            l2s[1].OneToOne_Required_FK_Inverse2 = l1s[8];
            l2s[0].OneToOne_Required_FK_Inverse2 = l1s[9];
            l2s[10].OneToOne_Required_FK_Inverse2 = l1s[10];

            l2s[9].Level1_Required_Id = l1s[0].Id;
            l2s[8].Level1_Required_Id = l1s[1].Id;
            l2s[7].Level1_Required_Id = l1s[2].Id;
            l2s[6].Level1_Required_Id = l1s[3].Id;
            l2s[5].Level1_Required_Id = l1s[4].Id;
            l2s[4].Level1_Required_Id = l1s[5].Id;
            l2s[3].Level1_Required_Id = l1s[6].Id;
            l2s[2].Level1_Required_Id = l1s[7].Id;
            l2s[1].Level1_Required_Id = l1s[8].Id;
            l2s[0].Level1_Required_Id = l1s[9].Id;
            l2s[10].Level1_Required_Id = l1s[10].Id;
        }

        l2s[0].OneToMany_Required_Inverse2 = l1s[0];
        l2s[1].OneToMany_Required_Inverse2 = l1s[0];
        l2s[2].OneToMany_Required_Inverse2 = l1s[0];
        l2s[3].OneToMany_Required_Inverse2 = l1s[0];
        l2s[4].OneToMany_Required_Inverse2 = l1s[0];
        l2s[5].OneToMany_Required_Inverse2 = l1s[0];
        l2s[6].OneToMany_Required_Inverse2 = l1s[0];
        l2s[7].OneToMany_Required_Inverse2 = l1s[0];
        l2s[8].OneToMany_Required_Inverse2 = l1s[0];
        l2s[9].OneToMany_Required_Inverse2 = l1s[0];
        if (!tableSplitting)
        {
            l2s[10].OneToMany_Required_Inverse2 = l1s[0];
        }

        l1s[0].OneToMany_Required_Self_Inverse1 = l1s[0];
        l1s[1].OneToMany_Required_Self_Inverse1 = l1s[0];
        l1s[2].OneToMany_Required_Self_Inverse1 = l1s[1];
        l1s[3].OneToMany_Required_Self_Inverse1 = l1s[2];
        l1s[4].OneToMany_Required_Self_Inverse1 = l1s[3];
        l1s[5].OneToMany_Required_Self_Inverse1 = l1s[4];
        l1s[6].OneToMany_Required_Self_Inverse1 = l1s[5];
        l1s[7].OneToMany_Required_Self_Inverse1 = l1s[6];
        l1s[8].OneToMany_Required_Self_Inverse1 = l1s[7];
        l1s[9].OneToMany_Required_Self_Inverse1 = l1s[8];
        if (!tableSplitting)
        {
            l1s[11].OneToMany_Required_Self_Inverse1 = l1s[0];
            l1s[12].OneToMany_Required_Self_Inverse1 = l1s[1];
            l1s[10].OneToMany_Required_Self_Inverse1 = l1s[10];
        }

        l3s[0].OneToOne_Required_PK_Inverse3 = l2s[0];
        l3s[1].OneToOne_Required_PK_Inverse3 = l2s[1];
        l3s[2].OneToOne_Required_PK_Inverse3 = l2s[2];
        l3s[3].OneToOne_Required_PK_Inverse3 = l2s[3];
        l3s[4].OneToOne_Required_PK_Inverse3 = l2s[4];
        l3s[5].OneToOne_Required_PK_Inverse3 = l2s[5];
        l3s[6].OneToOne_Required_PK_Inverse3 = l2s[6];
        l3s[7].OneToOne_Required_PK_Inverse3 = l2s[7];
        l3s[8].OneToOne_Required_PK_Inverse3 = l2s[8];
        l3s[9].OneToOne_Required_PK_Inverse3 = l2s[9];

        if (tableSplitting)
        {
            l3s[0].OneToOne_Required_FK_Inverse3 = l2s[0];
            l3s[1].OneToOne_Required_FK_Inverse3 = l2s[1];
            l3s[2].OneToOne_Required_FK_Inverse3 = l2s[2];
            l3s[3].OneToOne_Required_FK_Inverse3 = l2s[3];
            l3s[4].OneToOne_Required_FK_Inverse3 = l2s[4];
            l3s[5].OneToOne_Required_FK_Inverse3 = l2s[5];
            l3s[6].OneToOne_Required_FK_Inverse3 = l2s[6];
            l3s[7].OneToOne_Required_FK_Inverse3 = l2s[7];
            l3s[8].OneToOne_Required_FK_Inverse3 = l2s[8];
            l3s[9].OneToOne_Required_FK_Inverse3 = l2s[9];

            l3s[0].Level2_Required_Id = l2s[0].Id;
            l3s[1].Level2_Required_Id = l2s[1].Id;
            l3s[2].Level2_Required_Id = l2s[2].Id;
            l3s[3].Level2_Required_Id = l2s[3].Id;
            l3s[4].Level2_Required_Id = l2s[4].Id;
            l3s[5].Level2_Required_Id = l2s[5].Id;
            l3s[6].Level2_Required_Id = l2s[6].Id;
            l3s[7].Level2_Required_Id = l2s[7].Id;
            l3s[8].Level2_Required_Id = l2s[8].Id;
            l3s[9].Level2_Required_Id = l2s[9].Id;
        }
        else
        {
            l3s[9].OneToOne_Required_FK_Inverse3 = l2s[0];
            l3s[8].OneToOne_Required_FK_Inverse3 = l2s[1];
            l3s[7].OneToOne_Required_FK_Inverse3 = l2s[2];
            l3s[6].OneToOne_Required_FK_Inverse3 = l2s[3];
            l3s[5].OneToOne_Required_FK_Inverse3 = l2s[4];
            l3s[4].OneToOne_Required_FK_Inverse3 = l2s[5];
            l3s[3].OneToOne_Required_FK_Inverse3 = l2s[6];
            l3s[2].OneToOne_Required_FK_Inverse3 = l2s[7];
            l3s[1].OneToOne_Required_FK_Inverse3 = l2s[8];
            l3s[0].OneToOne_Required_FK_Inverse3 = l2s[9];

            l3s[9].Level2_Required_Id = l2s[0].Id;
            l3s[8].Level2_Required_Id = l2s[1].Id;
            l3s[7].Level2_Required_Id = l2s[2].Id;
            l3s[6].Level2_Required_Id = l2s[3].Id;
            l3s[5].Level2_Required_Id = l2s[4].Id;
            l3s[4].Level2_Required_Id = l2s[5].Id;
            l3s[3].Level2_Required_Id = l2s[6].Id;
            l3s[2].Level2_Required_Id = l2s[7].Id;
            l3s[1].Level2_Required_Id = l2s[8].Id;
            l3s[0].Level2_Required_Id = l2s[9].Id;
        }

        l3s[0].OneToMany_Required_Inverse3 = l2s[0];
        l3s[1].OneToMany_Required_Inverse3 = l2s[0];
        l3s[2].OneToMany_Required_Inverse3 = l2s[0];
        l3s[3].OneToMany_Required_Inverse3 = l2s[0];
        l3s[4].OneToMany_Required_Inverse3 = l2s[0];
        l3s[5].OneToMany_Required_Inverse3 = l2s[0];
        l3s[6].OneToMany_Required_Inverse3 = l2s[0];
        l3s[7].OneToMany_Required_Inverse3 = l2s[0];
        l3s[8].OneToMany_Required_Inverse3 = l2s[0];
        l3s[9].OneToMany_Required_Inverse3 = l2s[0];

        l2s[0].OneToMany_Required_Self_Inverse2 = l2s[0];
        l2s[1].OneToMany_Required_Self_Inverse2 = l2s[0];
        if (!tableSplitting)
        {
            l2s[10].OneToMany_Required_Self_Inverse2 = l2s[0];
        }

        l2s[2].OneToMany_Required_Self_Inverse2 = l2s[1];
        l2s[3].OneToMany_Required_Self_Inverse2 = l2s[2];
        l2s[4].OneToMany_Required_Self_Inverse2 = l2s[3];
        l2s[5].OneToMany_Required_Self_Inverse2 = l2s[4];
        l2s[6].OneToMany_Required_Self_Inverse2 = l2s[5];
        l2s[7].OneToMany_Required_Self_Inverse2 = l2s[6];
        l2s[8].OneToMany_Required_Self_Inverse2 = l2s[7];
        l2s[9].OneToMany_Required_Self_Inverse2 = l2s[8];

        l4s[0].OneToOne_Required_PK_Inverse4 = l3s[0];
        l4s[1].OneToOne_Required_PK_Inverse4 = l3s[1];
        l4s[2].OneToOne_Required_PK_Inverse4 = l3s[2];
        l4s[3].OneToOne_Required_PK_Inverse4 = l3s[3];
        l4s[4].OneToOne_Required_PK_Inverse4 = l3s[4];
        l4s[5].OneToOne_Required_PK_Inverse4 = l3s[5];
        l4s[6].OneToOne_Required_PK_Inverse4 = l3s[6];
        l4s[7].OneToOne_Required_PK_Inverse4 = l3s[7];
        l4s[8].OneToOne_Required_PK_Inverse4 = l3s[8];
        l4s[9].OneToOne_Required_PK_Inverse4 = l3s[9];

        if (tableSplitting)
        {
            l4s[0].OneToOne_Required_FK_Inverse4 = l3s[0];
            l4s[1].OneToOne_Required_FK_Inverse4 = l3s[1];
            l4s[2].OneToOne_Required_FK_Inverse4 = l3s[2];
            l4s[3].OneToOne_Required_FK_Inverse4 = l3s[3];
            l4s[4].OneToOne_Required_FK_Inverse4 = l3s[4];
            l4s[5].OneToOne_Required_FK_Inverse4 = l3s[5];
            l4s[6].OneToOne_Required_FK_Inverse4 = l3s[6];
            l4s[7].OneToOne_Required_FK_Inverse4 = l3s[7];
            l4s[8].OneToOne_Required_FK_Inverse4 = l3s[8];
            l4s[9].OneToOne_Required_FK_Inverse4 = l3s[9];

            l4s[0].Level3_Required_Id = l3s[0].Id;
            l4s[1].Level3_Required_Id = l3s[1].Id;
            l4s[2].Level3_Required_Id = l3s[2].Id;
            l4s[3].Level3_Required_Id = l3s[3].Id;
            l4s[4].Level3_Required_Id = l3s[4].Id;
            l4s[5].Level3_Required_Id = l3s[5].Id;
            l4s[6].Level3_Required_Id = l3s[6].Id;
            l4s[7].Level3_Required_Id = l3s[7].Id;
            l4s[8].Level3_Required_Id = l3s[8].Id;
            l4s[9].Level3_Required_Id = l3s[9].Id;
        }
        else
        {
            l4s[9].OneToOne_Required_FK_Inverse4 = l3s[0];
            l4s[8].OneToOne_Required_FK_Inverse4 = l3s[1];
            l4s[7].OneToOne_Required_FK_Inverse4 = l3s[2];
            l4s[6].OneToOne_Required_FK_Inverse4 = l3s[3];
            l4s[5].OneToOne_Required_FK_Inverse4 = l3s[4];
            l4s[4].OneToOne_Required_FK_Inverse4 = l3s[5];
            l4s[3].OneToOne_Required_FK_Inverse4 = l3s[6];
            l4s[2].OneToOne_Required_FK_Inverse4 = l3s[7];
            l4s[1].OneToOne_Required_FK_Inverse4 = l3s[8];
            l4s[0].OneToOne_Required_FK_Inverse4 = l3s[9];

            l4s[9].Level3_Required_Id = l3s[0].Id;
            l4s[8].Level3_Required_Id = l3s[1].Id;
            l4s[7].Level3_Required_Id = l3s[2].Id;
            l4s[6].Level3_Required_Id = l3s[3].Id;
            l4s[5].Level3_Required_Id = l3s[4].Id;
            l4s[4].Level3_Required_Id = l3s[5].Id;
            l4s[3].Level3_Required_Id = l3s[6].Id;
            l4s[2].Level3_Required_Id = l3s[7].Id;
            l4s[1].Level3_Required_Id = l3s[8].Id;
            l4s[0].Level3_Required_Id = l3s[9].Id;
        }

        l4s[0].OneToMany_Required_Inverse4 = l3s[0];
        l4s[1].OneToMany_Required_Inverse4 = l3s[0];
        l4s[2].OneToMany_Required_Inverse4 = l3s[0];
        l4s[3].OneToMany_Required_Inverse4 = l3s[0];
        l4s[4].OneToMany_Required_Inverse4 = l3s[0];
        l4s[5].OneToMany_Required_Inverse4 = l3s[0];
        l4s[6].OneToMany_Required_Inverse4 = l3s[0];
        l4s[7].OneToMany_Required_Inverse4 = l3s[0];
        l4s[8].OneToMany_Required_Inverse4 = l3s[0];
        l4s[9].OneToMany_Required_Inverse4 = l3s[0];

        l3s[0].OneToMany_Required_Self_Inverse3 = l3s[0];
        l3s[1].OneToMany_Required_Self_Inverse3 = l3s[0];
        l3s[2].OneToMany_Required_Self_Inverse3 = l3s[1];
        l3s[3].OneToMany_Required_Self_Inverse3 = l3s[2];
        l3s[4].OneToMany_Required_Self_Inverse3 = l3s[3];
        l3s[5].OneToMany_Required_Self_Inverse3 = l3s[4];
        l3s[6].OneToMany_Required_Self_Inverse3 = l3s[5];
        l3s[7].OneToMany_Required_Self_Inverse3 = l3s[6];
        l3s[8].OneToMany_Required_Self_Inverse3 = l3s[7];
        l3s[9].OneToMany_Required_Self_Inverse3 = l3s[8];

        l4s[0].OneToMany_Required_Self_Inverse4 = l4s[0];
        l4s[1].OneToMany_Required_Self_Inverse4 = l4s[0];
        l4s[2].OneToMany_Required_Self_Inverse4 = l4s[1];
        l4s[3].OneToMany_Required_Self_Inverse4 = l4s[2];
        l4s[4].OneToMany_Required_Self_Inverse4 = l4s[3];
        l4s[5].OneToMany_Required_Self_Inverse4 = l4s[4];
        l4s[6].OneToMany_Required_Self_Inverse4 = l4s[5];
        l4s[7].OneToMany_Required_Self_Inverse4 = l4s[6];
        l4s[8].OneToMany_Required_Self_Inverse4 = l4s[7];
        l4s[9].OneToMany_Required_Self_Inverse4 = l4s[8];
    }

    private static void WireUpPart2(
        IReadOnlyList<Level1> l1s,
        IReadOnlyList<Level2> l2s,
        IReadOnlyList<Level3> l3s,
        IReadOnlyList<Level4> l4s,
        bool tableSplitting)
    {
        l1s[0].OneToOne_Optional_PK1 = l2s[0];
        l1s[2].OneToOne_Optional_PK1 = l2s[2];
        l1s[4].OneToOne_Optional_PK1 = l2s[4];
        l1s[6].OneToOne_Optional_PK1 = l2s[6];
        l1s[8].OneToOne_Optional_PK1 = l2s[8];

        l1s[1].OneToOne_Optional_FK1 = l2s[8];
        l1s[3].OneToOne_Optional_FK1 = l2s[6];
        l1s[5].OneToOne_Optional_FK1 = l2s[4];
        l1s[7].OneToOne_Optional_FK1 = l2s[2];
        l1s[9].OneToOne_Optional_FK1 = l2s[0];

        l1s[0].OneToMany_Optional1 = new List<Level2>
        {
            l2s[1],
            l2s[3],
            l2s[5],
            l2s[7],
            l2s[9]
        };

        l1s[1].OneToMany_Optional_Self1 = new List<Level1> { l1s[0] };
        l1s[3].OneToMany_Optional_Self1 = new List<Level1> { l1s[2] };
        l1s[5].OneToMany_Optional_Self1 = new List<Level1> { l1s[4] };
        l1s[7].OneToMany_Optional_Self1 = new List<Level1> { l1s[6] };
        l1s[9].OneToMany_Optional_Self1 = new List<Level1> { l1s[8] };

        l1s[0].OneToOne_Optional_Self1 = l1s[9];
        l1s[1].OneToOne_Optional_Self1 = l1s[8];
        l1s[2].OneToOne_Optional_Self1 = l1s[7];
        l1s[3].OneToOne_Optional_Self1 = l1s[6];
        l1s[4].OneToOne_Optional_Self1 = l1s[5];

        l2s[0].OneToOne_Optional_PK2 = l3s[0];
        l2s[2].OneToOne_Optional_PK2 = l3s[2];
        l2s[5].OneToOne_Optional_PK2 = l3s[4];
        l2s[7].OneToOne_Optional_PK2 = l3s[6];
        l2s[9].OneToOne_Optional_PK2 = l3s[8];

        l2s[1].OneToOne_Optional_FK2 = l3s[8];
        l2s[3].OneToOne_Optional_FK2 = l3s[6];
        l2s[4].OneToOne_Optional_FK2 = l3s[4];
        l2s[6].OneToOne_Optional_FK2 = l3s[2];
        l2s[8].OneToOne_Optional_FK2 = l3s[0];

        l2s[0].OneToMany_Optional2 = new List<Level3>
        {
            l3s[1],
            l3s[5],
            l3s[9]
        };
        l2s[1].OneToMany_Optional2 = new List<Level3> { l3s[3], l3s[7] };

        l2s[1].OneToMany_Optional_Self2 = new List<Level2> { l2s[0] };
        l2s[3].OneToMany_Optional_Self2 = new List<Level2> { l2s[2] };
        l2s[5].OneToMany_Optional_Self2 = new List<Level2> { l2s[4] };
        l2s[7].OneToMany_Optional_Self2 = new List<Level2> { l2s[6] };
        l2s[9].OneToMany_Optional_Self2 = new List<Level2> { l2s[8] };

        l2s[0].OneToOne_Optional_Self2 = l2s[9];
        l2s[1].OneToOne_Optional_Self2 = l2s[8];
        l2s[2].OneToOne_Optional_Self2 = l2s[7];
        l2s[3].OneToOne_Optional_Self2 = l2s[6];
        l2s[4].OneToOne_Optional_Self2 = l2s[5];

        l3s[0].OneToOne_Optional_PK3 = l4s[0];
        l3s[2].OneToOne_Optional_PK3 = l4s[2];
        l3s[4].OneToOne_Optional_PK3 = l4s[4];
        l3s[6].OneToOne_Optional_PK3 = l4s[6];
        l3s[8].OneToOne_Optional_PK3 = l4s[8];

        l3s[1].OneToOne_Optional_FK3 = l4s[8];
        l3s[3].OneToOne_Optional_FK3 = l4s[6];
        l3s[5].OneToOne_Optional_FK3 = l4s[4];
        l3s[7].OneToOne_Optional_FK3 = l4s[2];
        l3s[9].OneToOne_Optional_FK3 = l4s[0];

        l3s[0].OneToMany_Optional3 = new List<Level4>
        {
            l4s[1],
            l4s[3],
            l4s[5],
            l4s[7],
            l4s[9]
        };

        l3s[1].OneToMany_Optional_Self3 = new List<Level3> { l3s[0] };
        l3s[3].OneToMany_Optional_Self3 = new List<Level3> { l3s[2] };
        l3s[5].OneToMany_Optional_Self3 = new List<Level3> { l3s[4] };
        l3s[7].OneToMany_Optional_Self3 = new List<Level3> { l3s[6] };
        l3s[9].OneToMany_Optional_Self3 = new List<Level3> { l3s[8] };

        l3s[0].OneToOne_Optional_Self3 = l3s[9];
        l3s[1].OneToOne_Optional_Self3 = l3s[8];
        l3s[2].OneToOne_Optional_Self3 = l3s[7];
        l3s[3].OneToOne_Optional_Self3 = l3s[6];
        l3s[4].OneToOne_Optional_Self3 = l3s[5];

        l4s[1].OneToMany_Optional_Self4 = new List<Level4> { l4s[0] };
        l4s[3].OneToMany_Optional_Self4 = new List<Level4> { l4s[2] };
        l4s[5].OneToMany_Optional_Self4 = new List<Level4> { l4s[4] };
        l4s[7].OneToMany_Optional_Self4 = new List<Level4> { l4s[6] };
        l4s[9].OneToMany_Optional_Self4 = new List<Level4> { l4s[8] };
    }

    private static void WireUpInversePart2(
        IReadOnlyList<Level1> l1s,
        IReadOnlyList<Level2> l2s,
        IReadOnlyList<Level3> l3s,
        IReadOnlyList<Level4> l4s,
        bool tableSplitting)
    {
        l2s[0].OneToOne_Optional_PK_Inverse2 = l1s[0];
        l2s[2].OneToOne_Optional_PK_Inverse2 = l1s[2];
        l2s[4].OneToOne_Optional_PK_Inverse2 = l1s[4];
        l2s[6].OneToOne_Optional_PK_Inverse2 = l1s[6];
        l2s[8].OneToOne_Optional_PK_Inverse2 = l1s[8];

        l2s[8].OneToOne_Optional_FK_Inverse2 = l1s[1];
        l2s[6].OneToOne_Optional_FK_Inverse2 = l1s[3];
        l2s[4].OneToOne_Optional_FK_Inverse2 = l1s[5];
        l2s[2].OneToOne_Optional_FK_Inverse2 = l1s[7];
        l2s[0].OneToOne_Optional_FK_Inverse2 = l1s[9];

        l2s[8].Level1_Optional_Id = l1s[1].Id;
        l2s[6].Level1_Optional_Id = l1s[3].Id;
        l2s[4].Level1_Optional_Id = l1s[5].Id;
        l2s[2].Level1_Optional_Id = l1s[7].Id;
        l2s[0].Level1_Optional_Id = l1s[9].Id;

        l2s[1].OneToMany_Optional_Inverse2 = l1s[0];
        l2s[3].OneToMany_Optional_Inverse2 = l1s[0];
        l2s[5].OneToMany_Optional_Inverse2 = l1s[0];
        l2s[7].OneToMany_Optional_Inverse2 = l1s[0];
        l2s[9].OneToMany_Optional_Inverse2 = l1s[0];

        l1s[0].OneToMany_Optional_Self_Inverse1 = l1s[1];
        l1s[2].OneToMany_Optional_Self_Inverse1 = l1s[3];
        l1s[4].OneToMany_Optional_Self_Inverse1 = l1s[5];
        l1s[6].OneToMany_Optional_Self_Inverse1 = l1s[7];
        l1s[8].OneToMany_Optional_Self_Inverse1 = l1s[9];

        l3s[0].OneToOne_Optional_PK_Inverse3 = l2s[0];
        l3s[2].OneToOne_Optional_PK_Inverse3 = l2s[2];
        l3s[4].OneToOne_Optional_PK_Inverse3 = l2s[5];
        l3s[6].OneToOne_Optional_PK_Inverse3 = l2s[7];
        l3s[8].OneToOne_Optional_PK_Inverse3 = l2s[9];

        l3s[8].OneToOne_Optional_FK_Inverse3 = l2s[1];
        l3s[6].OneToOne_Optional_FK_Inverse3 = l2s[3];
        l3s[4].OneToOne_Optional_FK_Inverse3 = l2s[4];
        l3s[2].OneToOne_Optional_FK_Inverse3 = l2s[6];
        l3s[0].OneToOne_Optional_FK_Inverse3 = l2s[8];

        l3s[8].Level2_Optional_Id = l2s[1].Id;
        l3s[6].Level2_Optional_Id = l2s[3].Id;
        l3s[4].Level2_Optional_Id = l2s[4].Id;
        l3s[2].Level2_Optional_Id = l2s[6].Id;
        l3s[0].Level2_Optional_Id = l2s[8].Id;

        l3s[1].OneToMany_Optional_Inverse3 = l2s[0];
        l3s[5].OneToMany_Optional_Inverse3 = l2s[0];
        l3s[9].OneToMany_Optional_Inverse3 = l2s[0];
        l3s[3].OneToMany_Optional_Inverse3 = l2s[1];
        l3s[7].OneToMany_Optional_Inverse3 = l2s[1];

        l2s[0].OneToMany_Optional_Self_Inverse2 = l2s[1];
        l2s[2].OneToMany_Optional_Self_Inverse2 = l2s[3];
        l2s[4].OneToMany_Optional_Self_Inverse2 = l2s[5];
        l2s[6].OneToMany_Optional_Self_Inverse2 = l2s[7];
        l2s[8].OneToMany_Optional_Self_Inverse2 = l2s[9];

        l4s[0].OneToOne_Optional_PK_Inverse4 = l3s[0];
        l4s[2].OneToOne_Optional_PK_Inverse4 = l3s[2];
        l4s[4].OneToOne_Optional_PK_Inverse4 = l3s[4];
        l4s[6].OneToOne_Optional_PK_Inverse4 = l3s[6];
        l4s[8].OneToOne_Optional_PK_Inverse4 = l3s[8];

        l4s[8].OneToOne_Optional_FK_Inverse4 = l3s[1];
        l4s[6].OneToOne_Optional_FK_Inverse4 = l3s[3];
        l4s[4].OneToOne_Optional_FK_Inverse4 = l3s[5];
        l4s[2].OneToOne_Optional_FK_Inverse4 = l3s[7];
        l4s[0].OneToOne_Optional_FK_Inverse4 = l3s[9];

        l4s[8].Level3_Optional_Id = l3s[1].Id;
        l4s[6].Level3_Optional_Id = l3s[3].Id;
        l4s[4].Level3_Optional_Id = l3s[5].Id;
        l4s[2].Level3_Optional_Id = l3s[7].Id;
        l4s[0].Level3_Optional_Id = l3s[9].Id;

        l4s[1].OneToMany_Optional_Inverse4 = l3s[0];
        l4s[3].OneToMany_Optional_Inverse4 = l3s[0];
        l4s[5].OneToMany_Optional_Inverse4 = l3s[0];
        l4s[7].OneToMany_Optional_Inverse4 = l3s[0];
        l4s[9].OneToMany_Optional_Inverse4 = l3s[0];

        l3s[0].OneToMany_Optional_Self_Inverse3 = l3s[1];
        l3s[2].OneToMany_Optional_Self_Inverse3 = l3s[3];
        l3s[4].OneToMany_Optional_Self_Inverse3 = l3s[5];
        l3s[6].OneToMany_Optional_Self_Inverse3 = l3s[7];
        l3s[8].OneToMany_Optional_Self_Inverse3 = l3s[9];

        l4s[0].OneToMany_Optional_Self_Inverse4 = l4s[1];
        l4s[2].OneToMany_Optional_Self_Inverse4 = l4s[3];
        l4s[4].OneToMany_Optional_Self_Inverse4 = l4s[5];
        l4s[6].OneToMany_Optional_Self_Inverse4 = l4s[7];
        l4s[8].OneToMany_Optional_Self_Inverse4 = l4s[9];
    }

    public static async Task SeedAsync(ComplexNavigationsContext context, bool tableSplitting = false)
    {
        var l1s = CreateLevelOnes(tableSplitting);
        var l2s = CreateLevelTwos(tableSplitting);
        var l3s = CreateLevelThrees(tableSplitting);
        var l4s = CreateLevelFours(tableSplitting);

        context.LevelOne.AddRange(l1s);

        WireUpPart1(l1s, l2s, l3s, l4s, tableSplitting);

        await context.SaveChangesAsync();

        WireUpPart2(l1s, l2s, l3s, l4s, tableSplitting);

        var globalizations = new List<ComplexNavigationGlobalization>();
        for (var i = 0; i < 10; i++)
        {
            var language = new ComplexNavigationLanguage { Name = "Language" + i, CultureString = "Foo" + i };
            var globalization = new ComplexNavigationGlobalization { Text = "Globalization" + i, Language = language };
            globalizations.Add(globalization);

            context.Languages.Add(language);
            context.Globalizations.Add(globalization);
        }

        var ib1s = CreateInheritanceBaseOnes();
        var ib2s = CreateInheritanceBaseTwos();
        var il1s = CreateInheritanceLeafOnes();
        var il2s = CreateInheritanceLeafTwos();

        context.InheritanceOne.AddRange(ib1s);
        context.InheritanceTwo.AddRange(ib2s);
        context.InheritanceLeafOne.AddRange(il1s);
        context.InheritanceLeafTwo.AddRange(il2s);

        WireUpInheritancePart1(ib1s, ib2s, il1s, il2s);
        await context.SaveChangesAsync();

        WireUpInheritancePart2(ib2s, il2s);
        await context.SaveChangesAsync();

        var mls1 = new ComplexNavigationString { DefaultText = "MLS1", Globalizations = globalizations.Take(3).ToList() };
        var mls2 = new ComplexNavigationString { DefaultText = "MLS2", Globalizations = globalizations.Skip(3).Take(3).ToList() };
        var mls3 = new ComplexNavigationString { DefaultText = "MLS3", Globalizations = globalizations.Skip(6).Take(3).ToList() };
        var mls4 = new ComplexNavigationString { DefaultText = "MLS4", Globalizations = globalizations.Skip(9).ToList() };

        context.MultilingualStrings.AddRange(mls1, mls2, mls3, mls4);

        var field1 = new ComplexNavigationField
        {
            Name = "Field1",
            Label = mls1,
            Placeholder = null
        };
        var field2 = new ComplexNavigationField
        {
            Name = "Field2",
            Label = mls3,
            Placeholder = mls4
        };

        context.Fields.AddRange(field1, field2);
        await context.SaveChangesAsync();
    }
}
