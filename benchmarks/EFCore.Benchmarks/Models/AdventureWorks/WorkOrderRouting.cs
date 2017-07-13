// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class WorkOrderRouting
    {
        public int WorkOrderID { get; set; }
        public int ProductID { get; set; }
        public short OperationSequence { get; set; }
        public decimal? ActualCost { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal? ActualResourceHrs { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public short LocationID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal PlannedCost { get; set; }
        public DateTime ScheduledEndDate { get; set; }
        public DateTime ScheduledStartDate { get; set; }

        public virtual Location Location { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }
    }
}
