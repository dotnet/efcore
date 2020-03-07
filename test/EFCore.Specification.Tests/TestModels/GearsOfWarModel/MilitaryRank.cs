// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
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
}
