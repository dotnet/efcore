// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

public class NullSemanticsData : ISetSource
{
    public static readonly NullSemanticsData Instance = new();

    private NullSemanticsData()
    {
        Entities1 = CreateEntities1();
        Entities2 = CreateEntities2();
    }

    public IReadOnlyList<NullSemanticsEntity1> Entities1 { get; }
    public IReadOnlyList<NullSemanticsEntity2> Entities2 { get; }

    public static IReadOnlyList<NullSemanticsEntity1> CreateEntities1()
        => CreateNullSemanticsEntityBases<NullSemanticsEntity1>();

    public static IReadOnlyList<NullSemanticsEntity2> CreateEntities2()
        => CreateNullSemanticsEntityBases<NullSemanticsEntity2>();

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(NullSemanticsEntity1))
        {
            return (IQueryable<TEntity>)Entities1.AsQueryable();
        }

        if (typeof(TEntity) == typeof(NullSemanticsEntity2))
        {
            return (IQueryable<TEntity>)Entities2.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    private static IReadOnlyList<TEntity> CreateNullSemanticsEntityBases<TEntity>()
        where TEntity : NullSemanticsEntityBase, new()
    {
        var nullableBoolValues = new bool?[] { false, true, null };
        var nullableStringValues = new[] { "Foo", "Bar", null };
        var nullableIntValues = new int?[] { 0, 1, null };

        var boolValues = new[] { false, true, true };
        var stringValues = new[] { "Foo", "Bar", "Bar" };
        var intValues = new[] { 0, 1, 2 };

        var entities = new List<TEntity>();

        var id = 0;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                for (var k = 0; k < 3; k++)
                {
                    id++;

                    var entity = new TEntity
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

                    entities.Add(entity);
                }
            }
        }

        return entities.ToArray();
    }
}
