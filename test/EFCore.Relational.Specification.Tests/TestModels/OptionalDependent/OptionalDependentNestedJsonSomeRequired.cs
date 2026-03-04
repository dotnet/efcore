// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

#nullable disable

public class OptionalDependentNestedJsonSomeRequired
{
    public string OpNested1 { get; set; }
    public int? OpNested2 { get; set; }

    public bool ReqNested1 { get; set; }
    public DateTime ReqNested2 { get; set; }
}
