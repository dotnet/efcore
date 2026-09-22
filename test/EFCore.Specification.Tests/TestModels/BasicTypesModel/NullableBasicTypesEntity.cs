// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

public class NullableBasicTypesEntity
{
    public required int Id { get; set; }

    public byte? Byte { get; set; }
    public short? Short { get; set; }
    public int? Int { get; set; }
    public long? Long { get; set; }
    public float? Float { get; set; }
    public double? Double { get; set; }
    public decimal? Decimal { get; set; }

    public string? String { get; set; }

    public DateTime? DateTime { get; set; }
    public DateOnly? DateOnly { get; set; }
    public TimeOnly? TimeOnly { get; set; }
    public DateTimeOffset? DateTimeOffset { get; set; }
    public TimeSpan? TimeSpan { get; set; }

    public bool? Bool { get; set; }
    public Guid? Guid { get; set; }
    public byte[]? ByteArray { get; set; }

    public BasicEnum? Enum { get; set; }
    public BasicFlagsEnum? FlagsEnum { get; set; }
}
