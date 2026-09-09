// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

#nullable disable

public abstract class NullSemanticsEntityBase
{
    public int Id { get; set; }
    public bool BoolA { get; set; }
    public bool BoolB { get; set; }
    public bool BoolC { get; set; }

    public bool? NullableBoolA { get; set; }
    public bool? NullableBoolB { get; set; }
    public bool? NullableBoolC { get; set; }

    public string StringA { get; set; }
    public string StringB { get; set; }
    public string StringC { get; set; }

    public string NullableStringA { get; set; }
    public string NullableStringB { get; set; }
    public string NullableStringC { get; set; }

    public int IntA { get; set; }
    public int IntB { get; set; }
    public int IntC { get; set; }

    public int? NullableIntA { get; set; }
    public int? NullableIntB { get; set; }
    public int? NullableIntC { get; set; }
}
