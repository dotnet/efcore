// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

public abstract class CompositeKeysData : ISetSource
{
    public IReadOnlyList<CompositeOne> CompositeOnes { get; }
    public IReadOnlyList<CompositeTwo> CompositeTwos { get; }
    public IReadOnlyList<CompositeThree> CompositeThrees { get; }
    public IReadOnlyList<CompositeFour> CompositeFours { get; }

    public abstract IQueryable<TEntity> Set<TEntity>()
        where TEntity : class;

    protected CompositeKeysData()
    {
        CompositeOnes = CreateCompositeOnes();
        CompositeTwos = CreateCompositeTwos();
        CompositeThrees = CreateCompositeThrees();
        CompositeFours = CreateCompositeFours();

        WireUpPart1(CompositeOnes, CompositeTwos, CompositeThrees, CompositeFours);
        WireUpInversePart1(CompositeOnes, CompositeTwos, CompositeThrees, CompositeFours);

        WireUpPart2(CompositeOnes, CompositeTwos, CompositeThrees, CompositeFours);
        WireUpInversePart2(CompositeOnes, CompositeTwos, CompositeThrees, CompositeFours);
    }

    public static IReadOnlyList<CompositeOne> CreateCompositeOnes()
    {
        var result = new List<CompositeOne>
        {
            new()
            {
                Id1 = "L0",
                Id2 = 1,
                Name = "L1 01",
                Date = new DateTime(2001, 1, 1)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 2,
                Name = "L1 02",
                Date = new DateTime(2002, 2, 2)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 3,
                Name = "L1 03",
                Date = new DateTime(2003, 3, 3)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 4,
                Name = "L1 04",
                Date = new DateTime(2004, 4, 4)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 5,
                Name = "L1 05",
                Date = new DateTime(2005, 5, 5)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 1,
                Name = "L1 06",
                Date = new DateTime(2006, 6, 6)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 2,
                Name = "L1 07",
                Date = new DateTime(2007, 7, 7)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 3,
                Name = "L1 08",
                Date = new DateTime(2008, 8, 8)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 4,
                Name = "L1 09",
                Date = new DateTime(2009, 9, 9)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 5,
                Name = "L1 10",
                Date = new DateTime(2010, 10, 10)
            },
            new()
            {
                Id1 = "L2",
                Id2 = 1,
                Name = "L1 11",
                Date = new DateTime(2009, 11, 11)
            },
            new()
            {
                Id1 = "L2",
                Id2 = 2,
                Name = "L1 12",
                Date = new DateTime(2008, 12, 12)
            },
            new()
            {
                Id1 = "L2",
                Id2 = 3,
                Name = "L1 13",
                Date = new DateTime(2007, 1, 1)
            }
        };

        foreach (var l1 in result)
        {
            l1.OneToMany_Optional1 = new List<CompositeTwo>();
            l1.OneToMany_Optional_Self1 = new List<CompositeOne>();
            l1.OneToMany_Required1 = new List<CompositeTwo>();
            l1.OneToMany_Required_Self1 = new List<CompositeOne>();
        }

        return result;
    }

    public static IReadOnlyList<CompositeTwo> CreateCompositeTwos()
    {
        var result = new List<CompositeTwo>
        {
            new()
            {
                Id1 = "L0",
                Id2 = 1,
                Name = "L2 01",
                Date = new DateTime(2010, 10, 10)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 2,
                Name = "L2 02",
                Date = new DateTime(2002, 2, 2)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 3,
                Name = "L2 03",
                Date = new DateTime(2008, 8, 8)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 4,
                Name = "L2 04",
                Date = new DateTime(2004, 4, 4)
            },
            new()
            {
                Id1 = "L0",
                Id2 = 5,
                Name = "L2 05",
                Date = new DateTime(2006, 6, 6)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 1,
                Name = "L2 06",
                Date = new DateTime(2005, 5, 5)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 2,
                Name = "L2 07",
                Date = new DateTime(2007, 7, 7)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 3,
                Name = "L2 08",
                Date = new DateTime(2003, 3, 3)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 4,
                Name = "L2 09",
                Date = new DateTime(2009, 9, 9)
            },
            new()
            {
                Id1 = "L1",
                Id2 = 5,
                Name = "L2 10",
                Date = new DateTime(2001, 1, 1)
            },
            new()
            {
                Id1 = "L2",
                Id2 = 1,
                Name = "L2 11",
                Date = new DateTime(2000, 1, 1)
            }
        };

        foreach (var l2 in result)
        {
            l2.OneToMany_Optional2 = new List<CompositeThree>();
            l2.OneToMany_Optional_Self2 = new List<CompositeTwo>();
            l2.OneToMany_Required2 = new List<CompositeThree>();
            l2.OneToMany_Required_Self2 = new List<CompositeTwo>();
        }

        return result;
    }

    public static IReadOnlyList<CompositeThree> CreateCompositeThrees()
    {
        var result = new List<CompositeThree>
        {
            new()
            {
                Id1 = "L0",
                Id2 = 1,
                Name = "L3 01"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 2,
                Name = "L3 02"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 3,
                Name = "L3 03"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 4,
                Name = "L3 04"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 5,
                Name = "L3 05"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 1,
                Name = "L3 06"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 2,
                Name = "L3 07"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 3,
                Name = "L3 08"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 4,
                Name = "L3 09"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 5,
                Name = "L3 10"
            }
        };

        foreach (var l3 in result)
        {
            l3.OneToMany_Optional3 = new List<CompositeFour>();
            l3.OneToMany_Optional_Self3 = new List<CompositeThree>();
            l3.OneToMany_Required3 = new List<CompositeFour>();
            l3.OneToMany_Required_Self3 = new List<CompositeThree>();
        }

        return result;
    }

    public static IReadOnlyList<CompositeFour> CreateCompositeFours()
    {
        var result = new List<CompositeFour>
        {
            new()
            {
                Id1 = "L0",
                Id2 = 1,
                Name = "L4 01"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 2,
                Name = "L4 02"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 3,
                Name = "L4 03"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 4,
                Name = "L4 04"
            },
            new()
            {
                Id1 = "L0",
                Id2 = 5,
                Name = "L4 05"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 1,
                Name = "L4 06"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 2,
                Name = "L4 07"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 3,
                Name = "L4 08"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 4,
                Name = "L4 09"
            },
            new()
            {
                Id1 = "L1",
                Id2 = 5,
                Name = "L4 10"
            }
        };

        foreach (var l4 in result)
        {
            l4.OneToMany_Optional_Self4 = new List<CompositeFour>();
            l4.OneToMany_Required_Self4 = new List<CompositeFour>();
        }

        return result;
    }

    private static void WireUpPart1(
        IReadOnlyList<CompositeOne> l1s,
        IReadOnlyList<CompositeTwo> l2s,
        IReadOnlyList<CompositeThree> l3s,
        IReadOnlyList<CompositeFour> l4s)
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
        l1s[10].OneToOne_Required_PK1 = l2s[10];

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

        l1s[0].OneToMany_Required1 = new List<CompositeTwo>
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

        l1s[0].OneToMany_Required1.Add(l2s[10]);
        l1s[0].OneToMany_Required_Self1 = new List<CompositeOne>
        {
            l1s[0],
            l1s[1],
            l1s[11]
        };
        l1s[1].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[2], l1s[12] };
        l1s[2].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[3] };
        l1s[3].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[4] };
        l1s[4].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[5] };
        l1s[5].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[6] };
        l1s[6].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[7] };
        l1s[7].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[8] };
        l1s[8].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[9] };
        l1s[9].OneToMany_Required_Self1 = new List<CompositeOne>();
        l1s[10].OneToMany_Required_Self1 = new List<CompositeOne> { l1s[10] };
        l1s[11].OneToMany_Required_Self1 = new List<CompositeOne>();
        l1s[12].OneToMany_Required_Self1 = new List<CompositeOne>();

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

        l2s[0].OneToMany_Required2 = new List<CompositeThree>
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

        l2s[0].OneToMany_Required_Self2 = new List<CompositeTwo>
        {
            l2s[0],
            l2s[1],
            l2s[10]
        };
        l2s[1].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[2] };
        l2s[2].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[3] };
        l2s[3].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[4] };
        l2s[4].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[5] };
        l2s[5].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[6] };
        l2s[6].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[7] };
        l2s[7].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[8] };
        l2s[8].OneToMany_Required_Self2 = new List<CompositeTwo> { l2s[9] };
        l2s[9].OneToMany_Required_Self2 = new List<CompositeTwo>();
        l2s[10].OneToMany_Required_Self2 = new List<CompositeTwo>();

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

        l3s[0].OneToMany_Required3 = new List<CompositeFour>
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

        l3s[0].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[0], l3s[1] };
        l3s[1].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[2] };
        l3s[2].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[3] };
        l3s[3].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[4] };
        l3s[4].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[5] };
        l3s[5].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[6] };
        l3s[6].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[7] };
        l3s[7].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[8] };
        l3s[8].OneToMany_Required_Self3 = new List<CompositeThree> { l3s[9] };
        l3s[9].OneToMany_Required_Self3 = new List<CompositeThree>();

        l4s[0].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[0], l4s[1] };
        l4s[1].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[2] };
        l4s[2].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[3] };
        l4s[3].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[4] };
        l4s[4].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[5] };
        l4s[5].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[6] };
        l4s[6].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[7] };
        l4s[7].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[8] };
        l4s[8].OneToMany_Required_Self4 = new List<CompositeFour> { l4s[9] };
        l4s[9].OneToMany_Required_Self4 = new List<CompositeFour>();
    }

    private static void WireUpInversePart1(
        IReadOnlyList<CompositeOne> l1s,
        IReadOnlyList<CompositeTwo> l2s,
        IReadOnlyList<CompositeThree> l3s,
        IReadOnlyList<CompositeFour> l4s)
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
        l2s[10].OneToOne_Required_PK_Inverse2 = l1s[10];

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

        l2s[9].Level1_Required_Id1 = l1s[0].Id1;
        l2s[9].Level1_Required_Id2 = l1s[0].Id2;
        l2s[8].Level1_Required_Id1 = l1s[1].Id1;
        l2s[8].Level1_Required_Id2 = l1s[1].Id2;
        l2s[7].Level1_Required_Id1 = l1s[2].Id1;
        l2s[7].Level1_Required_Id2 = l1s[2].Id2;
        l2s[6].Level1_Required_Id1 = l1s[3].Id1;
        l2s[6].Level1_Required_Id2 = l1s[3].Id2;
        l2s[5].Level1_Required_Id1 = l1s[4].Id1;
        l2s[5].Level1_Required_Id2 = l1s[4].Id2;
        l2s[4].Level1_Required_Id1 = l1s[5].Id1;
        l2s[4].Level1_Required_Id2 = l1s[5].Id2;
        l2s[3].Level1_Required_Id1 = l1s[6].Id1;
        l2s[3].Level1_Required_Id2 = l1s[6].Id2;
        l2s[2].Level1_Required_Id1 = l1s[7].Id1;
        l2s[2].Level1_Required_Id2 = l1s[7].Id2;
        l2s[1].Level1_Required_Id1 = l1s[8].Id1;
        l2s[1].Level1_Required_Id2 = l1s[8].Id2;
        l2s[0].Level1_Required_Id1 = l1s[9].Id1;
        l2s[0].Level1_Required_Id2 = l1s[9].Id2;
        l2s[10].Level1_Required_Id1 = l1s[10].Id1;
        l2s[10].Level1_Required_Id2 = l1s[10].Id2;

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
        l2s[10].OneToMany_Required_Inverse2 = l1s[0];

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
        l1s[11].OneToMany_Required_Self_Inverse1 = l1s[0];
        l1s[12].OneToMany_Required_Self_Inverse1 = l1s[1];
        l1s[10].OneToMany_Required_Self_Inverse1 = l1s[10];

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

        l3s[9].Level2_Required_Id1 = l2s[0].Id1;
        l3s[9].Level2_Required_Id2 = l2s[0].Id2;
        l3s[8].Level2_Required_Id1 = l2s[1].Id1;
        l3s[8].Level2_Required_Id2 = l2s[1].Id2;
        l3s[7].Level2_Required_Id1 = l2s[2].Id1;
        l3s[7].Level2_Required_Id2 = l2s[2].Id2;
        l3s[6].Level2_Required_Id1 = l2s[3].Id1;
        l3s[6].Level2_Required_Id2 = l2s[3].Id2;
        l3s[5].Level2_Required_Id1 = l2s[4].Id1;
        l3s[5].Level2_Required_Id2 = l2s[4].Id2;
        l3s[4].Level2_Required_Id1 = l2s[5].Id1;
        l3s[4].Level2_Required_Id2 = l2s[5].Id2;
        l3s[3].Level2_Required_Id1 = l2s[6].Id1;
        l3s[3].Level2_Required_Id2 = l2s[6].Id2;
        l3s[2].Level2_Required_Id1 = l2s[7].Id1;
        l3s[2].Level2_Required_Id2 = l2s[7].Id2;
        l3s[1].Level2_Required_Id1 = l2s[8].Id1;
        l3s[1].Level2_Required_Id2 = l2s[8].Id2;
        l3s[0].Level2_Required_Id1 = l2s[9].Id1;
        l3s[0].Level2_Required_Id2 = l2s[9].Id2;

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
        l2s[10].OneToMany_Required_Self_Inverse2 = l2s[0];

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

        l4s[9].Level3_Required_Id1 = l3s[0].Id1;
        l4s[9].Level3_Required_Id2 = l3s[0].Id2;
        l4s[8].Level3_Required_Id1 = l3s[1].Id1;
        l4s[8].Level3_Required_Id2 = l3s[1].Id2;
        l4s[7].Level3_Required_Id1 = l3s[2].Id1;
        l4s[7].Level3_Required_Id2 = l3s[2].Id2;
        l4s[6].Level3_Required_Id1 = l3s[3].Id1;
        l4s[6].Level3_Required_Id2 = l3s[3].Id2;
        l4s[5].Level3_Required_Id1 = l3s[4].Id1;
        l4s[5].Level3_Required_Id2 = l3s[4].Id2;
        l4s[4].Level3_Required_Id1 = l3s[5].Id1;
        l4s[4].Level3_Required_Id2 = l3s[5].Id2;
        l4s[3].Level3_Required_Id1 = l3s[6].Id1;
        l4s[3].Level3_Required_Id2 = l3s[6].Id2;
        l4s[2].Level3_Required_Id1 = l3s[7].Id1;
        l4s[2].Level3_Required_Id2 = l3s[7].Id2;
        l4s[1].Level3_Required_Id1 = l3s[8].Id1;
        l4s[1].Level3_Required_Id2 = l3s[8].Id2;
        l4s[0].Level3_Required_Id1 = l3s[9].Id1;
        l4s[0].Level3_Required_Id2 = l3s[9].Id2;

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
        IReadOnlyList<CompositeOne> l1s,
        IReadOnlyList<CompositeTwo> l2s,
        IReadOnlyList<CompositeThree> l3s,
        IReadOnlyList<CompositeFour> l4s)
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

        l1s[0].OneToMany_Optional1 = new List<CompositeTwo>
        {
            l2s[1],
            l2s[3],
            l2s[5],
            l2s[7],
            l2s[9]
        };

        l1s[1].OneToMany_Optional_Self1 = new List<CompositeOne> { l1s[0] };
        l1s[3].OneToMany_Optional_Self1 = new List<CompositeOne> { l1s[2] };
        l1s[5].OneToMany_Optional_Self1 = new List<CompositeOne> { l1s[4] };
        l1s[7].OneToMany_Optional_Self1 = new List<CompositeOne> { l1s[6] };
        l1s[9].OneToMany_Optional_Self1 = new List<CompositeOne> { l1s[8] };

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

        l2s[0].OneToMany_Optional2 = new List<CompositeThree>
        {
            l3s[1],
            l3s[5],
            l3s[9]
        };
        l2s[1].OneToMany_Optional2 = new List<CompositeThree> { l3s[3], l3s[7] };

        l2s[1].OneToMany_Optional_Self2 = new List<CompositeTwo> { l2s[0] };
        l2s[3].OneToMany_Optional_Self2 = new List<CompositeTwo> { l2s[2] };
        l2s[5].OneToMany_Optional_Self2 = new List<CompositeTwo> { l2s[4] };
        l2s[7].OneToMany_Optional_Self2 = new List<CompositeTwo> { l2s[6] };
        l2s[9].OneToMany_Optional_Self2 = new List<CompositeTwo> { l2s[8] };

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

        l3s[0].OneToMany_Optional3 = new List<CompositeFour>
        {
            l4s[1],
            l4s[3],
            l4s[5],
            l4s[7],
            l4s[9]
        };

        l3s[1].OneToMany_Optional_Self3 = new List<CompositeThree> { l3s[0] };
        l3s[3].OneToMany_Optional_Self3 = new List<CompositeThree> { l3s[2] };
        l3s[5].OneToMany_Optional_Self3 = new List<CompositeThree> { l3s[4] };
        l3s[7].OneToMany_Optional_Self3 = new List<CompositeThree> { l3s[6] };
        l3s[9].OneToMany_Optional_Self3 = new List<CompositeThree> { l3s[8] };

        l3s[0].OneToOne_Optional_Self3 = l3s[9];
        l3s[1].OneToOne_Optional_Self3 = l3s[8];
        l3s[2].OneToOne_Optional_Self3 = l3s[7];
        l3s[3].OneToOne_Optional_Self3 = l3s[6];
        l3s[4].OneToOne_Optional_Self3 = l3s[5];

        l4s[1].OneToMany_Optional_Self4 = new List<CompositeFour> { l4s[0] };
        l4s[3].OneToMany_Optional_Self4 = new List<CompositeFour> { l4s[2] };
        l4s[5].OneToMany_Optional_Self4 = new List<CompositeFour> { l4s[4] };
        l4s[7].OneToMany_Optional_Self4 = new List<CompositeFour> { l4s[6] };
        l4s[9].OneToMany_Optional_Self4 = new List<CompositeFour> { l4s[8] };
    }

    private static void WireUpInversePart2(
        IReadOnlyList<CompositeOne> l1s,
        IReadOnlyList<CompositeTwo> l2s,
        IReadOnlyList<CompositeThree> l3s,
        IReadOnlyList<CompositeFour> l4s)
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

        l2s[8].Level1_Optional_Id1 = l1s[1].Id1;
        l2s[8].Level1_Optional_Id2 = l1s[1].Id2;
        l2s[6].Level1_Optional_Id1 = l1s[3].Id1;
        l2s[6].Level1_Optional_Id2 = l1s[3].Id2;
        l2s[4].Level1_Optional_Id1 = l1s[5].Id1;
        l2s[4].Level1_Optional_Id2 = l1s[5].Id2;
        l2s[2].Level1_Optional_Id1 = l1s[7].Id1;
        l2s[2].Level1_Optional_Id2 = l1s[7].Id2;
        l2s[0].Level1_Optional_Id1 = l1s[9].Id1;
        l2s[0].Level1_Optional_Id2 = l1s[9].Id2;

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

        l3s[8].Level2_Optional_Id1 = l2s[1].Id1;
        l3s[8].Level2_Optional_Id2 = l2s[1].Id2;
        l3s[6].Level2_Optional_Id1 = l2s[3].Id1;
        l3s[6].Level2_Optional_Id2 = l2s[3].Id2;
        l3s[4].Level2_Optional_Id1 = l2s[4].Id1;
        l3s[4].Level2_Optional_Id2 = l2s[4].Id2;
        l3s[2].Level2_Optional_Id1 = l2s[6].Id1;
        l3s[2].Level2_Optional_Id2 = l2s[6].Id2;
        l3s[0].Level2_Optional_Id1 = l2s[8].Id1;
        l3s[0].Level2_Optional_Id2 = l2s[8].Id2;

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

        l4s[8].Level3_Optional_Id1 = l3s[1].Id1;
        l4s[8].Level3_Optional_Id2 = l3s[1].Id2;
        l4s[6].Level3_Optional_Id1 = l3s[3].Id1;
        l4s[6].Level3_Optional_Id2 = l3s[3].Id2;
        l4s[4].Level3_Optional_Id1 = l3s[5].Id1;
        l4s[4].Level3_Optional_Id2 = l3s[5].Id2;
        l4s[2].Level3_Optional_Id1 = l3s[7].Id1;
        l4s[2].Level3_Optional_Id2 = l3s[7].Id2;
        l4s[0].Level3_Optional_Id1 = l3s[9].Id1;
        l4s[0].Level3_Optional_Id2 = l3s[9].Id2;

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

    public static async Task SeedAsync(CompositeKeysContext context)
    {
        var l1s = CreateCompositeOnes();
        var l2s = CreateCompositeTwos();
        var l3s = CreateCompositeThrees();
        var l4s = CreateCompositeFours();

        context.CompositeOnes.AddRange(l1s);

        WireUpPart1(l1s, l2s, l3s, l4s);

        await context.SaveChangesAsync();

        WireUpPart2(l1s, l2s, l3s, l4s);

        await context.SaveChangesAsync();
    }
}
