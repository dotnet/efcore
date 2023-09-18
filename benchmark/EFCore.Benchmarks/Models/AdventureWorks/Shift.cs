// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Shift
{
    public Shift()
    {
        EmployeeDepartmentHistory = new HashSet<EmployeeDepartmentHistory>();
    }

    public byte ShiftID { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }
    public DateTime StartTime { get; set; }

    public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
}
