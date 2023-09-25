﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

public class OptionalDependentJsonSomeRequired
{
    public string OpProp1 { get; set; }
    public int? OpProp2 { get; set; }

    public double ReqProp { get; set; }

    public OptionalDependentNestedJsonAllOptional OpNested1 { get; set; }
    public OptionalDependentNestedJsonSomeRequired OpNested2 { get; set; }

    public OptionalDependentNestedJsonAllOptional ReqNested1 { get; set; }
    public OptionalDependentNestedJsonSomeRequired ReqNested2 { get; set; }
}
