// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

[Flags]
public enum BasicFlagsEnum
{
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Eight = 8,
    Sixteen = 16,
    ThirtyTwo = 32,
    OneTwentyEight = 128,
}
