// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class CreditCard
{
    public CreditCard()
    {
        PersonCreditCard = new HashSet<PersonCreditCard>();
        SalesOrderHeader = new HashSet<SalesOrderHeader>();
    }

    public int CreditCardID { get; set; }
    public string CardNumber { get; set; }
    public string CardType { get; set; }
    public byte ExpMonth { get; set; }
    public short ExpYear { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<PersonCreditCard> PersonCreditCard { get; set; }
    public virtual ICollection<SalesOrderHeader> SalesOrderHeader { get; set; }
}
