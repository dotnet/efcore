// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class PersonPhone
{
    public int BusinessEntityID { get; set; }
    public string PhoneNumber { get; set; }
    public int PhoneNumberTypeID { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Person BusinessEntity { get; set; }
    public virtual PhoneNumberType PhoneNumberType { get; set; }
}
