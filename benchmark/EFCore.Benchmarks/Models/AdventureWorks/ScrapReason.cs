// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class ScrapReason
    {
        public ScrapReason()
        {
            WorkOrder = new HashSet<WorkOrder>();
        }

        public short ScrapReasonID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<WorkOrder> WorkOrder { get; set; }
    }
}
