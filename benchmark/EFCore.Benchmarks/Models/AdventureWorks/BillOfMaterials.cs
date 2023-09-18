// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class BillOfMaterials
{
    public int BillOfMaterialsID { get; set; }
    public short BOMLevel { get; set; }
    public int ComponentID { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public decimal PerAssemblyQty { get; set; }
    public int? ProductAssemblyID { get; set; }
    public DateTime StartDate { get; set; }
    public string UnitMeasureCode { get; set; }

    public virtual Product Component { get; set; }
    public virtual Product ProductAssembly { get; set; }
    public virtual UnitMeasure UnitMeasureCodeNavigation { get; set; }
}
