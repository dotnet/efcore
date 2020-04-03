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

        public List<EntityTwo> TwoSkipShared { get; set; }

        public List<EntityThree> ThreeSkipFull { get; set; }
        public List<JoinThreeToCompositeKeyFull> JoinThreeFull { get; set; }

        public List<EntityRoot> RootSkipShared { get; set; }

        public List<EntityLeaf> LeafSkipFull { get; set; }
        public List<JoinCompositeKeyToLeaf> JoinLeafFull { get; set; }
    }
}
