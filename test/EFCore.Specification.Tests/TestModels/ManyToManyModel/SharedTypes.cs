// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    // TODO: remove and use shared type instead
    public class JoinOneToTwoShared
    {
        public virtual int OneId { get; set; }
        public virtual int TwoId { get; set; }
    }

    public class JoinOneToThreePayloadFullShared
    {
        public virtual int OneId { get; set; }
        public virtual int ThreeId { get; set; }
        public virtual string Payload { get; set; }
    }

    public class JoinTwoSelfShared
    {
        public virtual int LeftId { get; set; }
        public virtual int RightId { get; set; }
    }

    public class JoinTwoToCompositeKeyShared
    {
        public virtual int TwoId { get; set; }
        public virtual int CompositeId1 { get; set; }
        public virtual string CompositeId2 { get; set; }
        public virtual DateTime CompositeId3 { get; set; }
    }

    public class JoinThreeToRootShared
    {
        public virtual int ThreeId { get; set; }
        public virtual int RootId { get; set; }
    }

    public class JoinCompositeKeyToRootShared
    {
        public virtual int CompositeId1 { get; set; }
        public virtual string CompositeId2 { get; set; }
        public virtual DateTime CompositeId3 { get; set; }
        public virtual int RootId { get; set; }
    }

    public class ImplicitManyToManyA
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual ICollection<ImplicitManyToManyB> Bs { get; } = new ObservableCollection<ImplicitManyToManyB>();
    }

    public class ImplicitManyToManyB
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual ICollection<ImplicitManyToManyA> As { get; } = new ObservableCollection<ImplicitManyToManyA>();
    }
}
