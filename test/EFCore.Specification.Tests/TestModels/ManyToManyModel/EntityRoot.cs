// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityRoot
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ICollection<EntityThree> ThreeSkipShared { get; set; }
        public virtual ICollection<EntityCompositeKey> CompositeKeySkipShared { get; set; }
    }
}
