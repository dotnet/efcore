﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class InheritanceLeaf2
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<InheritanceBase2> BaseCollection { get; set; }
    }
}
