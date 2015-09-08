// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationsModelInitializer
    {
        public static void Seed(ComplexNavigationsContext context)
        {
            // TODO: only delete if model has changed
            context.Database.EnsureDeleted();
            if (context.Database.EnsureCreated())
            {
                var l1_01 = new Level1 { Id = 1, Name = "L1 01" };
                var l1_02 = new Level1 { Id = 2, Name = "L1 02" };
                var l1_03 = new Level1 { Id = 3, Name = "L1 03" };
                var l1_04 = new Level1 { Id = 4, Name = "L1 04" };
                var l1_05 = new Level1 { Id = 5, Name = "L1 05" };
                var l1_06 = new Level1 { Id = 6, Name = "L1 06" };
                var l1_07 = new Level1 { Id = 7, Name = "L1 07" };
                var l1_08 = new Level1 { Id = 8, Name = "L1 08" };
                var l1_09 = new Level1 { Id = 9, Name = "L1 09" };
                var l1_10 = new Level1 { Id = 10, Name = "L1 10" };

                var l2_01 = new Level2 { Id = 1, Name = "L2 01" };
                var l2_02 = new Level2 { Id = 2, Name = "L2 02" };
                var l2_03 = new Level2 { Id = 3, Name = "L2 03" };
                var l2_04 = new Level2 { Id = 4, Name = "L2 04" };
                var l2_05 = new Level2 { Id = 5, Name = "L2 05" };
                var l2_06 = new Level2 { Id = 6, Name = "L2 06" };
                var l2_07 = new Level2 { Id = 7, Name = "L2 07" };
                var l2_08 = new Level2 { Id = 8, Name = "L2 08" };
                var l2_09 = new Level2 { Id = 9, Name = "L2 09" };
                var l2_10 = new Level2 { Id = 10, Name = "L2 10" };

                var l3_01 = new Level3 { Id = 1, Name = "L3 01" };
                var l3_02 = new Level3 { Id = 2, Name = "L3 02" };
                var l3_03 = new Level3 { Id = 3, Name = "L3 03" };
                var l3_04 = new Level3 { Id = 4, Name = "L3 04" };
                var l3_05 = new Level3 { Id = 5, Name = "L3 05" };
                var l3_06 = new Level3 { Id = 6, Name = "L3 06" };
                var l3_07 = new Level3 { Id = 7, Name = "L3 07" };
                var l3_08 = new Level3 { Id = 8, Name = "L3 08" };
                var l3_09 = new Level3 { Id = 9, Name = "L3 09" };
                var l3_10 = new Level3 { Id = 10, Name = "L3 10" };

                var l4_01 = new Level4 { Id = 1, Name = "L4 01" };
                var l4_02 = new Level4 { Id = 2, Name = "L4 02" };
                var l4_03 = new Level4 { Id = 3, Name = "L4 03" };
                var l4_04 = new Level4 { Id = 4, Name = "L4 04" };
                var l4_05 = new Level4 { Id = 5, Name = "L4 05" };
                var l4_06 = new Level4 { Id = 6, Name = "L4 06" };
                var l4_07 = new Level4 { Id = 7, Name = "L4 07" };
                var l4_08 = new Level4 { Id = 8, Name = "L4 08" };
                var l4_09 = new Level4 { Id = 9, Name = "L4 09" };
                var l4_10 = new Level4 { Id = 10, Name = "L4 10" };

                var l1s = new[] { l1_01, l1_02, l1_03, l1_04, l1_05, l1_06, l1_07, l1_08, l1_09, l1_10 };
                var l2s = new[] { l2_01, l2_02, l2_03, l2_04, l2_05, l2_06, l2_07, l2_08, l2_09, l2_10 };
                var l3s = new[] { l3_01, l3_02, l3_03, l3_04, l3_05, l3_06, l3_07, l3_08, l3_09, l3_10 };
                var l4s = new[] { l4_01, l4_02, l4_03, l4_04, l4_05, l4_06, l4_07, l4_08, l4_09, l4_10 };

                context.LevelOne.AddRange(l1s);
                context.LevelTwo.AddRange(l2s);
                context.LevelThree.AddRange(l3s);
                context.LevelFour.AddRange(l4s);

                l1s[0].OneToOne_Required_PK = l2s[0];
                l1s[1].OneToOne_Required_PK = l2s[1];
                l1s[2].OneToOne_Required_PK = l2s[2];
                l1s[3].OneToOne_Required_PK = l2s[3];
                l1s[4].OneToOne_Required_PK = l2s[4];
                l1s[5].OneToOne_Required_PK = l2s[5];
                l1s[6].OneToOne_Required_PK = l2s[6];
                l1s[7].OneToOne_Required_PK = l2s[7];
                l1s[8].OneToOne_Required_PK = l2s[8];
                l1s[9].OneToOne_Required_PK = l2s[9];

                l1s[0].OneToOne_Required_FK = l2s[9];
                l1s[1].OneToOne_Required_FK = l2s[8];
                l1s[2].OneToOne_Required_FK = l2s[7];
                l1s[3].OneToOne_Required_FK = l2s[6];
                l1s[4].OneToOne_Required_FK = l2s[5];
                l1s[5].OneToOne_Required_FK = l2s[4];
                l1s[6].OneToOne_Required_FK = l2s[3];
                l1s[7].OneToOne_Required_FK = l2s[2];
                l1s[8].OneToOne_Required_FK = l2s[1];
                l1s[9].OneToOne_Required_FK = l2s[0];

                l1s[0].OneToMany_Required = new List<Level2> { l2s[0], l2s[1], l2s[2], l2s[3], l2s[4], l2s[5], l2s[6], l2s[7], l2s[8], l2s[9] };

                l1s[0].OneToMany_Required_Self = new List<Level1> { l1s[0], l1s[1] };
                l1s[1].OneToMany_Required_Self = new List<Level1> { l1s[2] };
                l1s[2].OneToMany_Required_Self = new List<Level1> { l1s[3] };
                l1s[3].OneToMany_Required_Self = new List<Level1> { l1s[4] };
                l1s[4].OneToMany_Required_Self = new List<Level1> { l1s[5] };
                l1s[5].OneToMany_Required_Self = new List<Level1> { l1s[6] };
                l1s[6].OneToMany_Required_Self = new List<Level1> { l1s[7] };
                l1s[7].OneToMany_Required_Self = new List<Level1> { l1s[8] };
                l1s[8].OneToMany_Required_Self = new List<Level1> { l1s[9] };
                l1s[9].OneToMany_Required_Self = new List<Level1>();

                l2s[0].OneToOne_Required_PK = l3s[0];
                l2s[1].OneToOne_Required_PK = l3s[1];
                l2s[2].OneToOne_Required_PK = l3s[2];
                l2s[3].OneToOne_Required_PK = l3s[3];
                l2s[4].OneToOne_Required_PK = l3s[4];
                l2s[5].OneToOne_Required_PK = l3s[5];
                l2s[6].OneToOne_Required_PK = l3s[6];
                l2s[7].OneToOne_Required_PK = l3s[7];
                l2s[8].OneToOne_Required_PK = l3s[8];
                l2s[9].OneToOne_Required_PK = l3s[9];

                l2s[0].OneToOne_Required_FK = l3s[9];
                l2s[1].OneToOne_Required_FK = l3s[8];
                l2s[2].OneToOne_Required_FK = l3s[7];
                l2s[3].OneToOne_Required_FK = l3s[6];
                l2s[4].OneToOne_Required_FK = l3s[5];
                l2s[5].OneToOne_Required_FK = l3s[4];
                l2s[6].OneToOne_Required_FK = l3s[3];
                l2s[7].OneToOne_Required_FK = l3s[2];
                l2s[8].OneToOne_Required_FK = l3s[1];
                l2s[9].OneToOne_Required_FK = l3s[0];

                l2s[0].OneToMany_Required = new List<Level3> { l3s[0], l3s[1], l3s[2], l3s[3], l3s[4], l3s[5], l3s[6], l3s[7], l3s[8], l3s[9] };

                l2s[0].OneToMany_Required_Self = new List<Level2> { l2s[0], l2s[1] };
                l2s[1].OneToMany_Required_Self = new List<Level2> { l2s[2] };
                l2s[2].OneToMany_Required_Self = new List<Level2> { l2s[3] };
                l2s[3].OneToMany_Required_Self = new List<Level2> { l2s[4] };
                l2s[4].OneToMany_Required_Self = new List<Level2> { l2s[5] };
                l2s[5].OneToMany_Required_Self = new List<Level2> { l2s[6] };
                l2s[6].OneToMany_Required_Self = new List<Level2> { l2s[7] };
                l2s[7].OneToMany_Required_Self = new List<Level2> { l2s[8] };
                l2s[8].OneToMany_Required_Self = new List<Level2> { l2s[9] };
                l2s[9].OneToMany_Required_Self = new List<Level2>();

                l3s[0].OneToOne_Required_PK = l4s[0];
                l3s[1].OneToOne_Required_PK = l4s[1];
                l3s[2].OneToOne_Required_PK = l4s[2];
                l3s[3].OneToOne_Required_PK = l4s[3];
                l3s[4].OneToOne_Required_PK = l4s[4];
                l3s[5].OneToOne_Required_PK = l4s[5];
                l3s[6].OneToOne_Required_PK = l4s[6];
                l3s[7].OneToOne_Required_PK = l4s[7];
                l3s[8].OneToOne_Required_PK = l4s[8];
                l3s[9].OneToOne_Required_PK = l4s[9];

                l3s[0].OneToOne_Required_FK = l4s[9];
                l3s[1].OneToOne_Required_FK = l4s[8];
                l3s[2].OneToOne_Required_FK = l4s[7];
                l3s[3].OneToOne_Required_FK = l4s[6];
                l3s[4].OneToOne_Required_FK = l4s[5];
                l3s[5].OneToOne_Required_FK = l4s[4];
                l3s[6].OneToOne_Required_FK = l4s[3];
                l3s[7].OneToOne_Required_FK = l4s[2];
                l3s[8].OneToOne_Required_FK = l4s[1];
                l3s[9].OneToOne_Required_FK = l4s[0];

                l3s[0].OneToMany_Required = new List<Level4> { l4s[0], l4s[1], l4s[2], l4s[3], l4s[4], l4s[5], l4s[6], l4s[7], l4s[8], l4s[9] };

                l3s[0].OneToMany_Required_Self = new List<Level3> { l3s[0], l3s[1] };
                l3s[1].OneToMany_Required_Self = new List<Level3> { l3s[2] };
                l3s[2].OneToMany_Required_Self = new List<Level3> { l3s[3] };
                l3s[3].OneToMany_Required_Self = new List<Level3> { l3s[4] };
                l3s[4].OneToMany_Required_Self = new List<Level3> { l3s[5] };
                l3s[5].OneToMany_Required_Self = new List<Level3> { l3s[6] };
                l3s[6].OneToMany_Required_Self = new List<Level3> { l3s[7] };
                l3s[7].OneToMany_Required_Self = new List<Level3> { l3s[8] };
                l3s[8].OneToMany_Required_Self = new List<Level3> { l3s[9] };
                l3s[9].OneToMany_Required_Self = new List<Level3>();

                l4s[0].OneToMany_Required_Self = new List<Level4> { l4s[0], l4s[1] };
                l4s[1].OneToMany_Required_Self = new List<Level4> { l4s[2] };
                l4s[2].OneToMany_Required_Self = new List<Level4> { l4s[3] };
                l4s[3].OneToMany_Required_Self = new List<Level4> { l4s[4] };
                l4s[4].OneToMany_Required_Self = new List<Level4> { l4s[5] };
                l4s[5].OneToMany_Required_Self = new List<Level4> { l4s[6] };
                l4s[6].OneToMany_Required_Self = new List<Level4> { l4s[7] };
                l4s[7].OneToMany_Required_Self = new List<Level4> { l4s[8] };
                l4s[8].OneToMany_Required_Self = new List<Level4> { l4s[9] };
                l4s[9].OneToMany_Required_Self = new List<Level4>();

                context.SaveChanges();

                l1s[0].OneToOne_Optional_PK = l2s[0];
                l1s[2].OneToOne_Optional_PK = l2s[2];
                l1s[4].OneToOne_Optional_PK = l2s[4];
                l1s[6].OneToOne_Optional_PK = l2s[6];
                l1s[8].OneToOne_Optional_PK = l2s[8];

                l1s[1].OneToOne_Optional_FK = l2s[8];
                l1s[3].OneToOne_Optional_FK = l2s[6];
                l1s[5].OneToOne_Optional_FK = l2s[4];
                l1s[7].OneToOne_Optional_FK = l2s[2];
                l1s[9].OneToOne_Optional_FK = l2s[0];

                l1s[0].OneToMany_Optional = new List<Level2> { l2s[1], l2s[3], l2s[5], l2s[7], l2s[9] };

                l1s[1].OneToMany_Optional_Self = new List<Level1> { l1s[0] };
                l1s[3].OneToMany_Optional_Self = new List<Level1> { l1s[2] };
                l1s[5].OneToMany_Optional_Self = new List<Level1> { l1s[4] };
                l1s[7].OneToMany_Optional_Self = new List<Level1> { l1s[6] };
                l1s[9].OneToMany_Optional_Self = new List<Level1> { l1s[8] };

                // issue #1417
                //l1s[0].OneToOne_Optional_Self = l1s[9];
                //l1s[1].OneToOne_Optional_Self = l1s[8];
                //l1s[2].OneToOne_Optional_Self = l1s[7];
                //l1s[3].OneToOne_Optional_Self = l1s[6];
                //l1s[4].OneToOne_Optional_Self = l1s[5];

                l2s[0].OneToOne_Optional_PK = l3s[0];
                l2s[2].OneToOne_Optional_PK = l3s[2];
                l2s[4].OneToOne_Optional_PK = l3s[4];
                l2s[6].OneToOne_Optional_PK = l3s[6];
                l2s[8].OneToOne_Optional_PK = l3s[8];

                l2s[1].OneToOne_Optional_FK = l3s[8];
                l2s[3].OneToOne_Optional_FK = l3s[6];
                l2s[5].OneToOne_Optional_FK = l3s[4];
                l2s[7].OneToOne_Optional_FK = l3s[2];
                l2s[9].OneToOne_Optional_FK = l3s[0];

                l2s[0].OneToMany_Optional = new List<Level3> { l3s[1], l3s[5], l3s[9] };
                l2s[1].OneToMany_Optional = new List<Level3> { l3s[3], l3s[7] };

                l2s[1].OneToMany_Optional_Self = new List<Level2> { l2s[0] };
                l2s[3].OneToMany_Optional_Self = new List<Level2> { l2s[2] };
                l2s[5].OneToMany_Optional_Self = new List<Level2> { l2s[4] };
                l2s[7].OneToMany_Optional_Self = new List<Level2> { l2s[6] };
                l2s[9].OneToMany_Optional_Self = new List<Level2> { l2s[8] };

                // issue #1417
                //l2s[0].OneToOne_Optional_Self = l2s[9];
                //l2s[1].OneToOne_Optional_Self = l2s[8];
                //l2s[2].OneToOne_Optional_Self = l2s[7];
                //l2s[3].OneToOne_Optional_Self = l2s[6];
                //l2s[4].OneToOne_Optional_Self = l2s[5];

                l3s[0].OneToOne_Optional_PK = l4s[0];
                l3s[2].OneToOne_Optional_PK = l4s[2];
                l3s[4].OneToOne_Optional_PK = l4s[4];
                l3s[6].OneToOne_Optional_PK = l4s[6];
                l3s[8].OneToOne_Optional_PK = l4s[8];

                l3s[1].OneToOne_Optional_FK = l4s[8];
                l3s[3].OneToOne_Optional_FK = l4s[6];
                l3s[5].OneToOne_Optional_FK = l4s[4];
                l3s[7].OneToOne_Optional_FK = l4s[2];
                l3s[9].OneToOne_Optional_FK = l4s[0];

                l3s[0].OneToMany_Optional = new List<Level4> { l4s[1], l4s[3], l4s[5], l4s[7], l4s[9] };

                l3s[1].OneToMany_Optional_Self = new List<Level3> { l3s[0] };
                l3s[3].OneToMany_Optional_Self = new List<Level3> { l3s[2] };
                l3s[5].OneToMany_Optional_Self = new List<Level3> { l3s[4] };
                l3s[7].OneToMany_Optional_Self = new List<Level3> { l3s[6] };
                l3s[9].OneToMany_Optional_Self = new List<Level3> { l3s[8] };

                // issue #1417
                //l3s[0].OneToOne_Optional_Self = l3s[9];
                //l3s[1].OneToOne_Optional_Self = l3s[8];
                //l3s[2].OneToOne_Optional_Self = l3s[7];
                //l3s[3].OneToOne_Optional_Self = l3s[6];
                //l3s[4].OneToOne_Optional_Self = l3s[5];

                l4s[1].OneToMany_Optional_Self = new List<Level4> { l4s[0] };
                l4s[3].OneToMany_Optional_Self = new List<Level4> { l4s[2] };
                l4s[5].OneToMany_Optional_Self = new List<Level4> { l4s[4] };
                l4s[7].OneToMany_Optional_Self = new List<Level4> { l4s[6] };
                l4s[9].OneToMany_Optional_Self = new List<Level4> { l4s[8] };

                context.SaveChanges();
            }
        }
    }
}
