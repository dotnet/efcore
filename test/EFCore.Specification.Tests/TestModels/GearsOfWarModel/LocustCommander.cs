// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class LocustCommander : LocustLeader
    {
        public LocustHorde CommandingFaction { get; set; }

        public string DefeatedByNickname { get; set; }
        public int? DefeatedBySquadId { get; set; }
        public Gear DefeatedBy { get; set; }

        public LocustHighCommand HighCommand { get; set; }
        public int HighCommandId { get; set; }
    }
}
