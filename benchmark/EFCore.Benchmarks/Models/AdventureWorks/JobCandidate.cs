// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class JobCandidate
{
    public int JobCandidateID { get; set; }
    public int? BusinessEntityID { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Resume { get; set; }

    public virtual Employee BusinessEntity { get; set; }
}
