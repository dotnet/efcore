// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityThree
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual int? ReferenceInverseId { get; set; }
        public virtual EntityTwo ReferenceInverse { get; set; }

        public virtual int? CollectionInverseId { get; set; }
        public virtual EntityTwo CollectionInverse { get; set; }

        public virtual ICollection<EntityOne> OneSkipPayloadFull { get; } = new ObservableCollection<EntityOne>(); // #21684

        public virtual ICollection<JoinOneToThreePayloadFull> JoinOnePayloadFull { get; }
            = new ObservableCollection<JoinOneToThreePayloadFull>(); // #21684

        public virtual ICollection<EntityTwo> TwoSkipFull { get; } = new ObservableCollection<EntityTwo>(); // #21684
        public virtual ICollection<JoinTwoToThree> JoinTwoFull { get; } = new ObservableCollection<JoinTwoToThree>(); // #21684

        public virtual ICollection<EntityOne> OneSkipPayloadFullShared { get; } = new ObservableCollection<EntityOne>(); // #21684

        public virtual ICollection<Dictionary<string, object>> JoinOnePayloadFullShared { get; }
             = new ObservableCollection<Dictionary<string, object>>(); // #21684

        public virtual ICollection<EntityCompositeKey> CompositeKeySkipFull { get; }
            = new ObservableCollection<EntityCompositeKey>(); // #21684

        public virtual ICollection<JoinThreeToCompositeKeyFull> JoinCompositeKeyFull { get; }
            = new ObservableCollection<JoinThreeToCompositeKeyFull>(); // #21684

        [InverseProperty("ThreeSkipShared")]
        public virtual ICollection<EntityRoot> RootSkipShared { get; } = new ObservableCollection<EntityRoot>(); // #21684
    }
}
