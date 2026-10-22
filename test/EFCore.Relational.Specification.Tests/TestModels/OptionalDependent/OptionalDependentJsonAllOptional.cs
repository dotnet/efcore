// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

#nullable disable

public class OptionalDependentJsonAllOptional
{
    public string OpProp1 { get; set; }
    public int? OpProp2 { get; set; }

    public OptionalDependentNestedJsonAllOptional OpNav1 { get; set; }
    public OptionalDependentNestedJsonSomeRequired OpNav2 { get; set; }
}
