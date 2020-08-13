// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class InheritanceDerived1 : InheritanceBase1
    {
        public InheritanceLeaf1 ReferenceSameType { get; set; }
        public InheritanceLeaf1 ReferenceDifferentType { get; set; }
        public List<InheritanceLeaf1> CollectionSameType { get; set; }
        public List<InheritanceLeaf1> CollectionDifferentType { get; set; }
    }
}
