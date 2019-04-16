// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel
{
    public class NullSemanticsContext : PoolableDbContext
    {
        public NullSemanticsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<NullSemanticsEntity1> Entities1 { get; set; }
        public DbSet<NullSemanticsEntity2> Entities2 { get; set; }

        public static void Seed(NullSemanticsContext context)
        {
            var nullableBoolValues = new bool?[] { false, true, null };
            var nullableStringValues = new[] { "Foo", "Bar", null };
            var nullableIntValues = new int?[] { 0, 1, null };

            var boolValues = new[] { false, true, true };
            var stringValues = new[] { "Foo", "Bar", "Bar" };
            var intValues = new[] { 0, 1, 2 };

            var entities1 = new List<NullSemanticsEntity1>();
            var entities2 = new List<NullSemanticsEntity2>();

            var id = 0;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        id++;

                        var entity1 = new NullSemanticsEntity1
                        {
                            Id = id,
                            BoolA = boolValues[i],
                            BoolB = boolValues[j],
                            BoolC = boolValues[k],
                            NullableBoolA = nullableBoolValues[i],
                            NullableBoolB = nullableBoolValues[j],
                            NullableBoolC = nullableBoolValues[k],
                            StringA = stringValues[i],
                            StringB = stringValues[j],
                            StringC = stringValues[k],
                            NullableStringA = nullableStringValues[i],
                            NullableStringB = nullableStringValues[j],
                            NullableStringC = nullableStringValues[k],
                            IntA = intValues[i],
                            IntB = intValues[j],
                            IntC = intValues[k],
                            NullableIntA = nullableIntValues[i],
                            NullableIntB = nullableIntValues[j],
                            NullableIntC = nullableIntValues[k]
                        };

                        var entity2 = new NullSemanticsEntity2
                        {
                            Id = id,
                            BoolA = boolValues[i],
                            BoolB = boolValues[j],
                            BoolC = boolValues[k],
                            NullableBoolA = nullableBoolValues[i],
                            NullableBoolB = nullableBoolValues[j],
                            NullableBoolC = nullableBoolValues[k],
                            StringA = stringValues[i],
                            StringB = stringValues[j],
                            StringC = stringValues[k],
                            NullableStringA = nullableStringValues[i],
                            NullableStringB = nullableStringValues[j],
                            NullableStringC = nullableStringValues[k],
                            IntA = intValues[i],
                            IntB = intValues[j],
                            IntC = intValues[k],
                            NullableIntA = nullableIntValues[i],
                            NullableIntB = nullableIntValues[j],
                            NullableIntC = nullableIntValues[k]
                        };

                        entities1.Add(entity1);
                        entities2.Add(entity2);
                    }
                }
            }

            context.Entities1.AddRange(entities1);
            context.Entities2.AddRange(entities2);
            context.SaveChanges();
        }
    }
}
