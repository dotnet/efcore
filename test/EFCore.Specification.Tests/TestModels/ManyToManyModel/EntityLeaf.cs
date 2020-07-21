// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityLeaf : EntityBranch
    {
        public virtual bool? IsGreen { get; set; }

        public virtual ICollection<EntityCompositeKey> CompositeKeySkipFull { get; }
            = new ObservableCollection<EntityCompositeKey>(); // #21684

        public virtual ICollection<JoinCompositeKeyToLeaf> JoinCompositeKeyFull { get; }
            = new ObservableCollection<JoinCompositeKeyToLeaf>(); // #21684
    }
}
