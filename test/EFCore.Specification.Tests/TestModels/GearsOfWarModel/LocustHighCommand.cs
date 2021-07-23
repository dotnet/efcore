﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class LocustHighCommand
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool IsOperational { get; set; }

        public List<LocustCommander> Commanders { get; set; }
    }
}
