// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityCompositeKey
    {
        public int Key1 { get; set; }
        public string Key2 { get; set; }
        public DateTime Key3 { get; set; }

        public string Name { get; set; }

        public List<EntityTwo> TwoSharedType { get; set; }
        public List<EntityThree> ThreeFullySpecified { get; set; }
        public List<EntityRoot> RootSharedType { get; set; }
        public List<EntityLeaf> LeafFullySpecified { get; set; }
    }
}
