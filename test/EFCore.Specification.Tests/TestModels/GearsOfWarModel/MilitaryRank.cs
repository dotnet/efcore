// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

[Flags]
public enum MilitaryRank
{
    None = 0,
    Private = 1,
    Corporal = 2,
    Sergeant = 4,
    Lieutenant = 8,
    Captain = 16,
    Major = 32,
    Colonel = 64,
    General = 128
}
