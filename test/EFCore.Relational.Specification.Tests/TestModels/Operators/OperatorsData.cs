// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Operators;

#nullable disable

public class OperatorsData : ISetSource
{
    public static readonly OperatorsData Instance = new();

    private readonly List<Expression<Func<string>>> _stringValues =
    [
        () => "A",
        () => "B",
        () => "AB"
    ];

    private readonly List<Expression<Func<int>>> _intValues =
    [
        () => 1,
        () => 2,
        () => 8
    ];

    private readonly List<Expression<Func<int?>>> _nullableIntValues =
    [
        () => null,
        () => 2,
        () => 8
    ];

    private readonly List<Expression<Func<long>>> _longValues =
    [
        () => 1L,
        () => 2L,
        () => 8L
    ];

    private readonly List<Expression<Func<bool>>> _boolValues =
    [
        () => true, () => false
    ];

    private readonly List<Expression<Func<bool?>>> _nullableBoolValues =
    [
        () => null,
        () => true,
        () => false
    ];

    private readonly List<Expression<Func<DateTimeOffset>>> _dateTimeOffsetValues =
    [
        () => new DateTimeOffset(new DateTime(2000, 1, 1, 11, 0, 0), new TimeSpan(5, 10, 0)),
        () => new DateTimeOffset(new DateTime(2000, 1, 1, 10, 0, 0), new TimeSpan(-8, 0, 0)),
        () => new DateTimeOffset(new DateTime(2000, 1, 1, 9, 0, 0), new TimeSpan(13, 0, 0))
    ];

    public IReadOnlyList<OperatorEntityString> OperatorEntitiesString { get; }
    public IReadOnlyList<OperatorEntityInt> OperatorEntitiesInt { get; }
    public IReadOnlyList<OperatorEntityNullableInt> OperatorEntitiesNullableInt { get; }
    public IReadOnlyList<OperatorEntityLong> OperatorEntitiesLong { get; }
    public IReadOnlyList<OperatorEntityBool> OperatorEntitiesBool { get; }
    public IReadOnlyList<OperatorEntityNullableBool> OperatorEntitiesNullableBool { get; }
    public IReadOnlyList<OperatorEntityDateTimeOffset> OperatorEntitiesDateTimeOffset { get; }
    public IDictionary<Type, List<Expression>> ConstantExpressionsPerType { get; }

    private OperatorsData()
    {
        OperatorEntitiesString = CreateStrings();
        OperatorEntitiesInt = CreateInts();
        OperatorEntitiesNullableInt = CreateNullableInts();
        OperatorEntitiesLong = CreateLongs();
        OperatorEntitiesBool = CreateBools();
        OperatorEntitiesNullableBool = CreateNullableBools();
        OperatorEntitiesDateTimeOffset = CreateDateTimeOffsets();

        ConstantExpressionsPerType = new Dictionary<Type, List<Expression>>
        {
            { typeof(string), _stringValues.Select(x => x.Body).ToList() },
            { typeof(int), _intValues.Select(x => x.Body).ToList() },
            { typeof(int?), _nullableIntValues.Select(x => x.Body).ToList() },
            { typeof(long), _longValues.Select(x => x.Body).ToList() },
            { typeof(bool), _boolValues.Select(x => x.Body).ToList() },
            { typeof(bool?), _nullableBoolValues.Select(x => x.Body).ToList() },
            { typeof(DateTimeOffset), _dateTimeOffsetValues.Select(x => x.Body).ToList() },
        };
    }

    public virtual IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(OperatorEntityString))
        {
            return (IQueryable<TEntity>)OperatorEntitiesString.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OperatorEntityInt))
        {
            return (IQueryable<TEntity>)OperatorEntitiesInt.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OperatorEntityNullableInt))
        {
            return (IQueryable<TEntity>)OperatorEntitiesNullableInt.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OperatorEntityLong))
        {
            return (IQueryable<TEntity>)OperatorEntitiesLong.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OperatorEntityBool))
        {
            return (IQueryable<TEntity>)OperatorEntitiesBool.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OperatorEntityNullableBool))
        {
            return (IQueryable<TEntity>)OperatorEntitiesNullableBool.AsQueryable();
        }

        if (typeof(TEntity) == typeof(OperatorEntityDateTimeOffset))
        {
            return (IQueryable<TEntity>)OperatorEntitiesDateTimeOffset.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public IReadOnlyList<OperatorEntityString> CreateStrings()
        => _stringValues.Select((x, i) => new OperatorEntityString { Id = i + 1, Value = _stringValues[i].Compile()() }).ToList();

    public IReadOnlyList<OperatorEntityInt> CreateInts()
        => _intValues.Select((x, i) => new OperatorEntityInt { Id = i + 1, Value = _intValues[i].Compile()() }).ToList();

    public IReadOnlyList<OperatorEntityNullableInt> CreateNullableInts()
        => _nullableIntValues.Select((x, i) => new OperatorEntityNullableInt { Id = i + 1, Value = _nullableIntValues[i].Compile()() })
            .ToList();

    public IReadOnlyList<OperatorEntityLong> CreateLongs()
        => _longValues.Select((x, i) => new OperatorEntityLong { Id = i + 1, Value = _longValues[i].Compile()() }).ToList();

    public IReadOnlyList<OperatorEntityBool> CreateBools()
        => _boolValues.Select((x, i) => new OperatorEntityBool { Id = i + 1, Value = _boolValues[i].Compile()() }).ToList();

    public IReadOnlyList<OperatorEntityNullableBool> CreateNullableBools()
        => _nullableBoolValues.Select((x, i) => new OperatorEntityNullableBool { Id = i + 1, Value = _nullableBoolValues[i].Compile()() })
            .ToList();

    public IReadOnlyList<OperatorEntityDateTimeOffset> CreateDateTimeOffsets()
        => _dateTimeOffsetValues
            .Select((x, i) => new OperatorEntityDateTimeOffset { Id = i + 1, Value = _dateTimeOffsetValues[i].Compile()() }).ToList();
}
