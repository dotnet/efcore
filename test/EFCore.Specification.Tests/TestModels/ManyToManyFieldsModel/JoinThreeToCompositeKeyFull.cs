// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class JoinThreeToCompositeKeyFull
    {
        public Guid Id;
        public int ThreeId;
        public int CompositeId1;
        public string CompositeId2;
        public DateTime CompositeId3;

        public EntityThree Three;
        public EntityCompositeKey Composite;
    }
}
