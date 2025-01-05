// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Person
{
    public Person()
    {
        BusinessEntityContact = new HashSet<BusinessEntityContact>();
        Customer = new HashSet<Customer>();
        EmailAddress = new HashSet<EmailAddress>();
        PersonCreditCard = new HashSet<PersonCreditCard>();
        PersonPhone = new HashSet<PersonPhone>();
    }

    public int BusinessEntityID { get; set; }
    public int EmailPromotion { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool NameStyle { get; set; }
    public string PersonType { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public string Suffix { get; set; }
    public string Title { get; set; }
    public string AdditionalContactInfo { get; set; }
    public string Demographics { get; set; }

    public virtual ICollection<BusinessEntityContact> BusinessEntityContact { get; set; }
    public virtual ICollection<Customer> Customer { get; set; }
    public virtual ICollection<EmailAddress> EmailAddress { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Password Password { get; set; }
    public virtual ICollection<PersonCreditCard> PersonCreditCard { get; set; }
    public virtual ICollection<PersonPhone> PersonPhone { get; set; }
    public virtual BusinessEntity BusinessEntity { get; set; }
}
