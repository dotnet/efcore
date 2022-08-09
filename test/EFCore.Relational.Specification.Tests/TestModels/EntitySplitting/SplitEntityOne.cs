// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

public class SplitEntityOne
{
    public int Id { get; set; }
    public string Value { get; set; }
    public int SharedValue { get; set; }
    public string SplitValue { get; set; }
}
