// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class City
    {
        // non-integer key with not conventional name
        public string Name { get; set; }

        public string Location { get; set; }

        public string Nation { get; set; }

        public List<Gear> BornGears { get; set; }
        public List<Gear> StationedGears { get; set; }
    }
}
