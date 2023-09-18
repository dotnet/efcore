// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Employee
{
    public Employee()
    {
        EmployeeDepartmentHistory = new HashSet<EmployeeDepartmentHistory>();
        EmployeePayHistory = new HashSet<EmployeePayHistory>();
        JobCandidate = new HashSet<JobCandidate>();
        PurchaseOrderHeader = new HashSet<PurchaseOrderHeader>();
    }

    public int BusinessEntityID { get; set; }
    public DateTime BirthDate { get; set; }
    public bool CurrentFlag { get; set; }
    public string Gender { get; set; }
    public DateTime HireDate { get; set; }
    public string JobTitle { get; set; }
    public string LoginID { get; set; }
    public string MaritalStatus { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string NationalIDNumber { get; set; }
    public short? OrganizationLevel { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public bool SalariedFlag { get; set; }
    public short SickLeaveHours { get; set; }
    public short VacationHours { get; set; }

    public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistory { get; set; }
    public virtual ICollection<EmployeePayHistory> EmployeePayHistory { get; set; }
    public virtual ICollection<JobCandidate> JobCandidate { get; set; }
    public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
    public virtual SalesPerson SalesPerson { get; set; }
    public virtual Person BusinessEntity { get; set; }
}
