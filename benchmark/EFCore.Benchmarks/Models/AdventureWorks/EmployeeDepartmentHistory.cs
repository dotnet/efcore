// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class EmployeeDepartmentHistory
{
    public int BusinessEntityID { get; set; }
    public DateTime StartDate { get; set; }
    public short DepartmentID { get; set; }
    public byte ShiftID { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Employee BusinessEntity { get; set; }
    public virtual Department Department { get; set; }
    public virtual Shift Shift { get; set; }
}
