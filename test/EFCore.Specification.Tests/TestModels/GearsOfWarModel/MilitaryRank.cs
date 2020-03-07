// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    [Flags]
    public enum MilitaryRank
    {
        Private = 0,
        Corporal = 1,
        Sergeant = 2,
        Lieutenant = 4,
        Captain = 8,
        Major = 16,
        Colonel = 32,
        General = 64
    }
}
