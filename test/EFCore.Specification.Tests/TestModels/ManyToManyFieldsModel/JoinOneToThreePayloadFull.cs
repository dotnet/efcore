// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class JoinOneToThreePayloadFull
    {
        public int OneId;
        public int ThreeId;
        public EntityOne One;
        public EntityThree Three;

        public string Payload;
    }
}
