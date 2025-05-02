// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

public class BasicTypesData : ISetSource
{
    public IReadOnlyList<BasicTypesEntity> BasicTypesEntities { get; } = CreateBasicTypesEntities();
    public IReadOnlyList<NullableBasicTypesEntity> NullableBasicTypesEntities { get; } = CreateNullableBasicTypesEntities();

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(BasicTypesEntity))
        {
            return (IQueryable<TEntity>)BasicTypesEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(NullableBasicTypesEntity))
        {
            return (IQueryable<TEntity>)NullableBasicTypesEntities.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<BasicTypesEntity> CreateBasicTypesEntities()
        =>
        [
            // TODO: min, max, some more "regular values"
            // TODO: go over, clean this up and make sure it makes sense
            new()
            {
                Id = 0,

                Byte = 0,
                Short = 0,
                Int = 0,
                Long = 0,
                Float = 0,
                Double = 0,
                Decimal = 0,

                String = string.Empty,

                DateTime = new DateTime(2000, 1, 1, 0, 0, 0),
                DateOnly = new DateOnly(2000, 1, 1),
                TimeOnly = new TimeOnly(0, 0, 0),
                DateTimeOffset = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                // TODO: Need to test non-zero days (not supported on SQL Server so need to tweak seeding data, which is currently hard)
                TimeSpan = TimeSpan.Zero,

                Bool = false,
                Guid = Guid.Empty,
                ByteArray = [],

                Enum = 0,
                FlagsEnum = BasicFlagsEnum.Zero,
            },
            new()
            {
                Id = 1,

                Byte = 8,
                Short = 8,
                Int = 8,
                Long = 8,
                Float = 8.6f,
                Double = 8.6,
                Decimal = 8.6m,

                String = "Seattle",

                DateTime = new DateTime(1998, 5, 4, 15, 30, 10),
                DateOnly = new DateOnly(2020, 1, 1),
                TimeOnly = new TimeOnly(15, 30, 10),
                // Note: we use a zero offset for the default seeded entity since a few providers don't support non-zero offsets; this
                // allows them to remove the non-zero-offset data from the seeding dataset, while leaving most tests - which don't care
                // about the offset - working.
                DateTimeOffset = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero),
                TimeSpan = new TimeSpan(1, 2, 3),

                Bool = true,
                Guid = new Guid("DF36F493-463F-4123-83F9-6B135DEEB7BA"),
                ByteArray = [0xDE, 0xAD, 0xBE, 0xEF],

                Enum = BasicEnum.One,
                FlagsEnum = BasicFlagsEnum.Eight,
            },
            new()
            {
                Id = 2,

                Byte = 8,
                Short = 8,
                Int = 8,
                Long = 8,
                Float = 8.6f,
                Double = 8.6,
                Decimal = 8.6m,

                String = "London",

                DateTime = new DateTime(1998, 5, 4, 15, 30, 10, 123, 456).AddTicks(400),
                DateOnly = new DateOnly(1990, 11, 10),
                TimeOnly = new TimeOnly(15, 30, 10, 123, 456),
                DateTimeOffset = new DateTimeOffset(1998, 5, 4, 15, 30, 10, 123, 456, TimeSpan.Zero)
                    .Add(TimeSpan.FromTicks(4)), /* 400 nanoseconds */
                TimeSpan = new TimeSpan(0, 3, 4, 5, 678, 912).Add(TimeSpan.FromTicks(4)), /* 400 nanoseconds */

                Bool = false,
                Guid = new Guid("B39A6FBA-9026-4D69-828E-FD7068673E57"),
                ByteArray = [1, 2, 3, 4, 5, 6, 7, 8, 9],

                Enum = BasicEnum.Two,
                FlagsEnum = BasicFlagsEnum.Eight | BasicFlagsEnum.One,
            },
            new()
            {
                Id = 3,

                Byte = 255,
                Short = 255,
                Int = 255,
                Long = 255,
                Float = 255.12f,
                Double = 255.12,
                Decimal = 255.12m,

                String = "Toronto",

                DateTime = new DateTime(1, 1, 1, 0, 0, 0),
                DateOnly = new DateOnly(1, 1, 1),
                TimeOnly = new TimeOnly(0, 0, 0),
                DateTimeOffset = new DateTimeOffset(1998, 5, 4, 15, 30, 10, 123, 456, new TimeSpan(1, 30, 0))
                    .Add(TimeSpan.FromTicks(4)), /* 400 nanoseconds */
                TimeSpan = new TimeSpan(0, 1, 0, 15, 456),

                // Bool = false,
                // Guid = new Guid("088ca6e6-c756-42f8-a298-8c28e63fdba6"),
                ByteArray = [],

                Enum = BasicEnum.Three,
                FlagsEnum = BasicFlagsEnum.Sixteen | BasicFlagsEnum.Four | BasicFlagsEnum.One,
            },
            new()
            {
                Id = 4,

                Byte = 9,
                Short = -9,
                Int = -9,
                Long = -9,
                Float = -9.5f,
                Double = -9.5,
                Decimal = -9.5m,

                // String = "  Boston  ",
                String = "  Boston  ",

                DateTime = new DateTime(1, 1, 1, 0, 0, 0, 10, 200).AddTicks(4), /* 400 nanoseconds */
                DateOnly = new DateOnly(1, 1, 1),
                TimeOnly = new TimeOnly(0, 0, 0, 10, 200).Add(TimeSpan.FromTicks(4)), /* 400 nanoseconds */
                DateTimeOffset = new DateTimeOffset(11, 5, 3, 12, 0, 0, 0, 200, new TimeSpan()).Add(TimeSpan.FromTicks(4)), /* 400 nanoseconds */
                TimeSpan = new TimeSpan(0, 2, 0, 15, 456, 200).Add(TimeSpan.FromTicks(4)), /* 400 nanoseconds */

                Bool = true,
                // Guid = new Guid("088ca6e6-c756-42f8-a298-8c28e63fdba6"),
                ByteArray = [],

                // Enum = BasicEnum.One,
                // FlagsEnum = BasicFlagsEnum.Eight,
            },
            new()
            {
                Id = 5,

                Byte = 12,
                Short = 12,
                Int = 12,
                Long = 12,
                Float = 12,
                Double = 12,
                Decimal = 12,

                String = "Berlin",

                // DateTime = DateTime.MinValue,
                // DateOnly = DateOnly.MinValue,
                // TimeOnly = TimeOnly.MinValue,
                // DateTimeOffset = DateTimeOffset.MinValue,
                // TimeSpan = TimeSpan.MinValue,

                Bool = false,
                // Guid = new Guid("088ca6e6-c756-42f8-a298-8c28e63fdba6"),
                ByteArray = [],

                // Enum = BasicEnum.One,
            },

            new()
            {
                Id = 6,

                Byte = 1,
                Short = 2,
                Int = 3,
                Long = 4,
                Float = 5.6f,
                Double = 6.7f,
                Decimal = 8.8m,

                String = "Seattle",

                // DateTime = DateTime.MinValue,
                // DateOnly = DateOnly.MinValue,
                // TimeOnly = TimeOnly.MinValue,
                // DateTimeOffset = DateTimeOffset.MinValue,
                // TimeSpan = TimeSpan.MinValue,

                Bool = false,
                // Guid = new Guid("088ca6e6-c756-42f8-a298-8c28e63fdba6"),
                ByteArray = [],

                // Enum = BasicEnum.One,
            }
        ];

    public static IReadOnlyList<NullableBasicTypesEntity> CreateNullableBasicTypesEntities()
    {
        // Convert the non-nullable data to nullable (so we have parity between the non-nullable and nullable seeding sets), and add another
        // entity instance with all-nulls.
        return CreateBasicTypesEntities()
            .Select(ConvertToNullable)
            .Append(new()
                {
                    Id = -1,

                    Byte = null,
                    Short = null,
                    Int = null,
                    Long = null,
                    Float = null,
                    Double = null,
                    Decimal = null,

                    String = null,

                    DateTime = null,
                    DateOnly = null,
                    TimeOnly = null,
                    DateTimeOffset = null,

                    Bool = null,
                    Guid = null,
                    ByteArray = null,

                    Enum = null,
                    FlagsEnum = null
                })
            .ToArray();

        NullableBasicTypesEntity ConvertToNullable(BasicTypesEntity b)
            => new()
            {
                Id = b.Id,

                Byte = b.Byte,
                Short = b.Short,
                Int = b.Int,
                Long = b.Long,
                Float = b.Float,
                Double = b.Double,
                Decimal = b.Decimal,

                String = b.String,

                DateTime = b.DateTime,
                DateOnly = b.DateOnly,
                TimeOnly = b.TimeOnly,
                DateTimeOffset = b.DateTimeOffset,
                TimeSpan = b.TimeSpan,

                Bool = b.Bool,
                Guid = b.Guid,
                ByteArray = b.ByteArray,

                Enum = b.Enum,
                FlagsEnum = b.FlagsEnum,
            };
    }
}
