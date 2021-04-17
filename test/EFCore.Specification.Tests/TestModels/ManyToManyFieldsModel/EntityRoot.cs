// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class EntityRoot
    {
        public int Id;
        public string Name;
        public ICollection<EntityThree> ThreeSkipShared;
        public ICollection<EntityCompositeKey> CompositeKeySkipShared;
    }
}
