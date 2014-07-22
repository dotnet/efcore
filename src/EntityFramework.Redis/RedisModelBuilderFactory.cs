// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisModelBuilderFactory : IModelBuilderFactory
    {
        public virtual ModelBuilder CreateConventionBuilder(Model model)
        {
            Check.NotNull(model, "model");

            //TODO custom conventions for Redis
            return new ConventionModelBuilder(model);
        }
    }
}
