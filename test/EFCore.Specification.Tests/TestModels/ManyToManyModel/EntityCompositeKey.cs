// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityCompositeKey
    {
        public virtual int Key1 { get; set; }
        public virtual string Key2 { get; set; }
        public virtual DateTime Key3 { get; set; }

        public virtual string Name { get; set; }

        public virtual ICollection<EntityTwo> TwoSkipShared { get; set; }
        public virtual ICollection<EntityThree> ThreeSkipFull { get; set; }
        public virtual ICollection<JoinThreeToCompositeKeyFull> JoinThreeFull { get; set; }
        public virtual ICollection<EntityRoot> RootSkipShared { get; set; }
        public virtual ICollection<EntityLeaf> LeafSkipFull { get; set; }
        public virtual ICollection<JoinCompositeKeyToLeaf> JoinLeafFull { get; set; }
    }
}
