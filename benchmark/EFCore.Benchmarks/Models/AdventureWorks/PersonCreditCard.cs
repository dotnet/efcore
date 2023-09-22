// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class PersonCreditCard
{
    public int BusinessEntityID { get; set; }
    public int CreditCardID { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Person BusinessEntity { get; set; }
    public virtual CreditCard CreditCard { get; set; }
}
