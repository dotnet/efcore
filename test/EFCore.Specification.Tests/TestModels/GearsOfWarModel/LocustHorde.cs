﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class LocustHorde : Faction
    {
        public LocustCommander Commander { get; set; }
        public List<LocustLeader> Leaders { get; set; }

        public string CommanderName { get; set; }
        public bool? Eradicated { get; set; }
    }
}
