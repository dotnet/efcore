// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class EntityCompositeKey
    {
        public int Key1;
        public string Key2;
        public DateTime Key3;

        public string Name;

        public ICollection<EntityTwo> TwoSkipShared;
        public ICollection<EntityThree> ThreeSkipFull;
        public ICollection<JoinThreeToCompositeKeyFull> JoinThreeFull;
        public ICollection<EntityRoot> RootSkipShared;
        public ICollection<EntityLeaf> LeafSkipFull;
        public ICollection<JoinCompositeKeyToLeaf> JoinLeafFull;
    }
}
