// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityCompositeKey
    {
        public virtual int Key1 { get; set; }
        public virtual string Key2 { get; set; }
        public virtual DateTime Key3 { get; set; }

        public virtual string Name { get; set; }

        public virtual ICollection<EntityTwo> TwoSkipShared { get; } = new ObservableCollection<EntityTwo>(); // #21684

        public virtual ICollection<EntityThree> ThreeSkipFull { get; } = new ObservableCollection<EntityThree>(); // #21684

        public virtual ICollection<JoinThreeToCompositeKeyFull> JoinThreeFull { get; }
            = new ObservableCollection<JoinThreeToCompositeKeyFull>(); // #21684

        public virtual ICollection<EntityRoot> RootSkipShared { get; } = new ObservableCollection<EntityRoot>(); // #21684

        public virtual ICollection<EntityLeaf> LeafSkipFull { get; } = new ObservableCollection<EntityLeaf>(); // #21684

        public virtual ICollection<JoinCompositeKeyToLeaf> JoinLeafFull { get; }
            = new ObservableCollection<JoinCompositeKeyToLeaf>(); // #21684
    }
}
