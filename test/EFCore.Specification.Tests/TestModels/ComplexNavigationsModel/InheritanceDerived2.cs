﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class InheritanceDerived2 : InheritanceBase1
    {
        public InheritanceLeaf1 ReferenceSameType { get; set; }
        public InheritanceLeaf2 ReferenceDifferentType { get; set; }
        public List<InheritanceLeaf1> CollectionSameType { get; set; }
        public List<InheritanceLeaf2> CollectionDifferentType { get; set; }
    }
}
