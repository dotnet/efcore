// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
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
}
