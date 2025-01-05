// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class WorkOrder
{
    public WorkOrder()
    {
        WorkOrderRouting = new HashSet<WorkOrderRouting>();
    }

    public int WorkOrderID { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int OrderQty { get; set; }
    public int ProductID { get; set; }
    public short ScrappedQty { get; set; }
    public short? ScrapReasonID { get; set; }
    public DateTime StartDate { get; set; }
    public int StockedQty { get; set; }

    public virtual ICollection<WorkOrderRouting> WorkOrderRouting { get; set; }
    public virtual Product Product { get; set; }
    public virtual ScrapReason ScrapReason { get; set; }
}
