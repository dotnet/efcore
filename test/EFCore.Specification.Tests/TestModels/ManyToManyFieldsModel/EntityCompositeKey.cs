﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
