// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class Officer : Gear
    {
        public Officer()
        {
            Reports = new List<Gear>();
        }

        // 1 - many self reference
        public virtual ICollection<Gear> Reports { get; set; }
    }
}
