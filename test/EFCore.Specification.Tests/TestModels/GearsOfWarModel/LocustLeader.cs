// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class LocustLeader
    {
        public string Name { get; set; }
        public short ThreatLevel { get; set; }
        public byte ThreatLevelByte { get; set; }
        public byte? ThreatLevelNullableByte { get; set; }
    }
}
