// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class ComplexType
{
    // For sorting when projecting out the complex type - must be unique
    public int UniqueInt { get; set; }

    public int Int { get; set; }
    public NestedComplexType? Nested { get; set; }
}

public class NestedComplexType
{
    // For sorting when projecting out the complex type - must be unique
    public int UniqueInt { get; set; }

    public int NestedInt { get; set; }
}
