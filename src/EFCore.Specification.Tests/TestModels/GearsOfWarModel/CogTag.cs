// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class CogTag
    {
        // auto generated key (identity for now)
        public Guid Id { get; set; }

        public string Note { get; set; }

        public string GearNickName { get; set; }
        public int? GearSquadId { get; set; }
        public virtual Gear Gear { get; set; }
    }
}
