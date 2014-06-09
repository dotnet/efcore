// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsValueGeneratorCache : ValueGeneratorCache
    {
        public override IValueGenerator GetGenerator(IProperty property)
        {
            //TODO Timestamp? ETag?
            return null;
        }
    }
}
