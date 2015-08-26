// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks
{
    public class JobCandidate
    {
        public int JobCandidateID { get; set; }
        public int? BusinessEntityID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Resume { get; set; }

        public virtual Employee BusinessEntity { get; set; }
    }
}
