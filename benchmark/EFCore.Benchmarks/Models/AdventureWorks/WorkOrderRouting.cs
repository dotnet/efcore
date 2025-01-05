// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

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
