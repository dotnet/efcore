// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class PhoneNumberType
{
    public PhoneNumberType()
    {
        PersonPhone = new HashSet<PersonPhone>();
    }

    public int PhoneNumberTypeID { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }

    public virtual ICollection<PersonPhone> PersonPhone { get; set; }
}
