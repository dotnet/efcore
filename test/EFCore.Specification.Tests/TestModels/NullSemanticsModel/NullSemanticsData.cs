// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel
{
    public class NullSemanticsData
    {
        public NullSemanticsData()
        {
            Initialize();
        }

        public NullSemanticsEntity1[] _entities1 { get; set; }
        public NullSemanticsEntity2[] _entities2 { get; set; }

        public void Initialize()
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

            _entities1 = entities1.ToArray();
            _entities2 = entities2.ToArray();
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : NullSemanticsEntityBase
        {
            if (typeof(TEntity) == typeof(NullSemanticsEntity1))
            {
                return _entities1.AsQueryable().Cast<TEntity>();
            }

            if (typeof(TEntity) == typeof(NullSemanticsEntity2))
            {
                return _entities2.AsQueryable().Cast<TEntity>();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }
    }
}
